using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using PhotonRoom = Photon.Realtime.Room;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using static Photon.Pun.PhotonNetwork;
using static PhotonManager;
using System.Linq;

public class GameManager : MonoBehaviourPunCallbacks, IOnEventCallback
{
    #region singleton
    private static GameManager instance;
    public static GameManager Instance
    {
        get
        {
            if (instance == null)
                instance = FindObjectOfType<GameManager>();
            return instance;
        }
    }
    #endregion

    #region field
    [SerializeField] private InGameUIManager uiManager;
    [SerializeField] private MissionManager missionManager;
    [SerializeField] private VoiceChatManager voiceManager;
    [SerializeField] private GameObject cameraObj;
    [SerializeField] private GhostController ghostPrefab;
    [SerializeField] private Light spotlight;
    [SerializeField] private GameObject[] doors;
    private Player target;
    private Transform[] spawnPositions;
    private Vector3 spawnPosition;
    private PlayerCharacter localCharacter;
    private bool canKill = true;
    private bool isVibrating = false;
    private bool userMute = false;
    private bool systemMute = false;
    private readonly Dictionary<int, PlayerCharacter> playerObjs = new Dictionary<int, PlayerCharacter>();
    #endregion

    #region property
    public bool IsMafia { get; private set; }
    public bool MenuOpened { get; set; } = false;
    public bool PhaseChanging { get; set; } = false;
    public bool IsVoting { get; private set; } = false;
    public bool IsMissionPlaying { get; set; } = false;
    public bool UserMute
    {
        get => userMute;
        set
        {
            userMute = value;
            voiceManager.SetMute(userMute || systemMute);
        }
    }
    private bool SystemMute
    {
        get => systemMute;
        set
        {
            systemMute = value;
            voiceManager.SetMute(userMute || systemMute);
        }
    }
    #endregion

    #region unity message
    private void Awake()
    {
        FindReference();
    }
    private void Start()
    {
        IsMessageQueueRunning = true;
        SpawnPlayer();
    }
    #endregion

    #region method

    #region private
    private void FindReference()
    {
        spawnPositions = transform.Find("SpawnPosition").GetComponentsInChildren<Transform>();
    }
    private void SpawnPlayer()
    {
        int idx = Array.IndexOf(PlayerList, LocalPlayer);
        spawnPosition = spawnPositions[idx + 1].position;
        localCharacter = PhotonNetwork.Instantiate(uiManager.isVR ? "PlayerVR" : "Player", spawnPosition, Quaternion.identity).GetComponent<PlayerCharacter>();
    }
    private void SetHelmetColor(PlayerCharacter character)
    {
        Material mat = character.transform.GetComponentInChildren<Head>().GetComponent<SkinnedMeshRenderer>().material;
        //mat.SetColor("_BaseColor", Global.colors[character.Owner.ActorNumber - 1]);
        mat.SetColor("_EmissionColor", Global.colors[character.Owner.ActorNumber - 1] * Mathf.LinearToGammaSpace(2f));
    }

    private IEnumerator Vibration(float frequency, float amplitude, float duration, bool isRight)
    {
        OVRInput.Controller controller = isRight ? OVRInput.Controller.RTouch : OVRInput.Controller.LTouch;
        float time = 0f;
        isVibrating = true;

        while (time < duration)
        {
            if ((time % 1.5f) == 0) OVRInput.SetControllerVibration(frequency, amplitude, controller);
            time += 0.5f;

            yield return new WaitForSeconds(0.5f);
        }

        OVRInput.SetControllerVibration(0, 0, controller);
        isVibrating = false;
    }
    #endregion

    #region public
    public void ReturnSpawnPosition()
    {
        if (LocalPlayer.GetAlive())
            localCharacter.Controller.MoveTo(spawnPosition);
    }

    public void MakeVibration(float frequency, float amplitude, float duration, bool isRight)
    {
        if (isVibrating) return;

        StartCoroutine(Vibration(frequency, amplitude, duration, isRight));
    }
    #endregion

    #endregion

    #region event handler

    #region message handler
    public void OnEvent(EventData photonEvent)
    {
        switch ((VrMafiaEventCode)photonEvent.Code)
        {
            #region phase
            case VrMafiaEventCode.GameStart:
                OnGameStarted(photonEvent);
                break;
            case VrMafiaEventCode.DayStart:
                OnDayStarted();
                break;
            case VrMafiaEventCode.NightStart:
                OnNightStarted(photonEvent);
                break;
            case VrMafiaEventCode.VotingStart:
                OnVotingStarted(photonEvent);
                break;
            case VrMafiaEventCode.VotingEnd:
                OnVotingEnded(photonEvent);
                break;
            case VrMafiaEventCode.DefenseStart:
                OnDefenseStarted(photonEvent);
                break;
            case VrMafiaEventCode.FinalVotingStart:
                OnFinalVotingStarted(photonEvent);
                break;
            case VrMafiaEventCode.FinalVotingEnd:
                OnFinalVotingEnded(photonEvent);
                break;
            case VrMafiaEventCode.GameEnd:
                OnGameEnded(photonEvent);
                break;
            #endregion

            #region response
            case VrMafiaEventCode.KillRes:
                OnKillResponse(photonEvent);
                break;
            case VrMafiaEventCode.DieEvent:
                OnDieEvent(photonEvent);
                break;
            case VrMafiaEventCode.KillReady:
                OnKillReady();
                break;
            case VrMafiaEventCode.VoteRes:
                OnVoteResponse(photonEvent);
                break;
            case VrMafiaEventCode.FinalVoteRes:
                OnFinalVoteResponse(photonEvent);
                break;
            #endregion

            default: break;
        }
    }

    #region phase
    private void OnGameStarted(EventData data)
    {
        var content = (Hashtable)data.CustomData;
        SystemMute = false;
        IsMafia = (bool)content["isMafia"];
        var mafiaIds = IsMafia ? (int[])content["mafiaIds"] : null;
        Debug.Log($"[GameManager] Game Start, Is Mafia : {IsMafia}");
        uiManager.OnGameStarted(IsMafia, mafiaIds);
    }
    private void OnDayStarted()
    {
        Debug.Log($"[GameManager] Day Start");
        IsVoting = false;
        uiManager.OnDayStarted();
        spotlight.intensity = 5;
        foreach (var d in doors)
            d.SetActive(false);

    }
    private void RemoveBody(int playerId)
    {
        var player = playerObjs[playerId];
        player.gameObject.layer = (int)Global.Layers.Ghost;
        player.Hide();
    }
    private void OnNightStarted(EventData data)
    {
        var deadId = (int)data.CustomData;
        Debug.Log($"[GameManager] Night Start : {deadId}");
        foreach (var p in PlayerList.Where(p => !p.GetAlive()))
        {
            RemoveBody(p.ActorNumber);
        }
        uiManager.OnNightStarted();
        spotlight.intensity = 1;
        foreach (var d in doors)
            d.SetActive(true);
    }
    private void OnVotingStarted(EventData data)
    {
        var votingTime = (float)data.CustomData;
        Debug.Log($"[GameManager] Voting Start : {votingTime}");
        IsVoting = true;
        uiManager.OnVotingStarted(votingTime);
    }
    private void OnVotingEnded(EventData data)
    {
        var content = (Hashtable)data.CustomData;
        var electedId = (int)content["electedId"];
        var result = (int[])content["result"];
        Debug.Log($"[GameManager] elected : {electedId}, result : {string.Join(" ", result)}");
        uiManager.OnVotingEnded(electedId, result);
    }
    private void OnDefenseStarted(EventData data)
    {
        var content = (Hashtable)data.CustomData;
        var electedId = (int)content["electedId"];
        var defenseTime = (float)content["defenseTime"];
        if (electedId != LocalPlayer.ActorNumber)
            SystemMute = true;
        Debug.Log($"[GameManager] Defense Start : {defenseTime}");
        uiManager.OnDefenseStarted(electedId, defenseTime);
    }
    private void OnFinalVotingStarted(EventData data)
    {
        if (LocalPlayer.GetAlive())
            SystemMute = false;
        var finalVotingTime = (float)data.CustomData;
        Debug.Log($"[GameManager] Final Voting Start : {finalVotingTime}");
        uiManager.OnFinalVotingStarted(finalVotingTime);
    }
    private void OnFinalVotingEnded(EventData data)
    {
        var content = (Hashtable)data.CustomData;
        var electedId = (int)content["electedId"];
        var pros = (int)content["pros"];
        Debug.Log($"[GameManager] result : {electedId}, pros : {pros}");
        if (electedId != -1)
        {
            PlayerDeath(electedId);
            RemoveBody(electedId);
        }
        uiManager.OnFinalVotingEnded(electedId, pros);
    }
    private void OnGameEnded(EventData data)
    {
        var content = (Hashtable)data.CustomData;
        var mafiaWin = (bool)content["mafiaWin"];
        var mafiaIds = (int[])content["mafiaIds"];
        Debug.Log($"[GameManager] Game End, Mafia Win : {mafiaWin}");
        uiManager.OnGameEnded(!(IsMafia ^ mafiaWin));
    }
    #endregion

    #region response
    private void OnKillResponse(EventData photonEvent)
    {
        var coolTime = (float)photonEvent.CustomData;
        Debug.Log($"[GameManager] Kill Response : {coolTime}");
        uiManager.OnKillResponse(coolTime);
        if (coolTime < 0)
            canKill = true;
    }
    private void OnKillReady()
    {
        Debug.Log($"[GameManager] Kill Ready");
        canKill = true;
    }
    private void PlayerDeath(int playerId)
    {
        var player = playerObjs[playerId];
        if (player.Owner.IsLocal)
        {
            Instantiate(ghostPrefab, player.transform.position, player.transform.rotation).InitGhost(cameraObj);
            SystemMute = true;
        }
        player.Die();
    }
    private void OnDieEvent(EventData data)
    {
        var content = (Hashtable)data.CustomData;
        var killerId = (int)content["killerId"];
        var targetId = (int)content["targetId"];
        playerObjs[killerId].Shoot();
        PlayerDeath(targetId);
        Debug.Log($"[GameManager] Die Event : {CurrentRoom.Players[targetId].NickName}");
    }
    private void OnVoteResponse(EventData data)
    {
        var result = (bool)data.CustomData;
        Debug.Log($"[GameManager] Vote Response : {result}");
        if (!result)
            uiManager.OnVoteFailed();
    }
    private void OnFinalVoteResponse(EventData data)
    {
        var result = (bool)data.CustomData;
        Debug.Log($"[GameManager] Final Vote Response : {result}");
        if (!result)
            uiManager.OnFinalVoteFailed();
    }
    #endregion

    #endregion

    #region ui handler
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        playerObjs.Remove(otherPlayer.ActorNumber);
    }
    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        if (!targetPlayer.IsLocal && changedProps.ContainsKey("VoiceName"))
        {
            voiceManager.MappingPlayer(targetPlayer.GetVoiceName(), playerObjs[targetPlayer.ActorNumber]);
        }
    }
    public void OnSpwanPlayer(PlayerCharacter obj)
    {
        SetHelmetColor(obj);

        playerObjs[obj.Owner.ActorNumber] = obj;
        if (obj.Owner.IsLocal)
        {
            SetVoiceName(voiceManager.LocalVoiceName);

            obj.InitLocalCharacter(cameraObj, uiManager.isVR);
            obj.Hide();
        }
    }
    public void OnFoundTarget(Player p)
    {
        target = p;
        uiManager.OnFoundTarget(p);
    }
    public void OnKillButton()
    {
        if (target == null ||
            !target.GetAlive() ||
            !LocalPlayer.GetAlive() ||
            !IsMafia ||
            !canKill)
            return;
        Debug.Log($"[GameManager] Kill Request : {target.NickName}");
        canKill = false;
        MakeVibration(0.5f, 1f, 1f, true);
        SendEventToMaster(VrMafiaEventCode.KillReq, target.ActorNumber);
    }
    public void OnDeadReportButton()
    {
        if (target == null ||
            target.GetAlive() ||
            !LocalPlayer.GetAlive())
            return;
        Debug.Log($"[GameManager] Dead Report : {target.NickName}");
        SendEventToMaster(VrMafiaEventCode.DeadReport, target.ActorNumber);
    }
    public void OnVoiceKey()
    {
        UserMute = !UserMute;
        Debug.Log("Voice : " + (UserMute ? "Off" : "On"));
        uiManager.OnVoiceKey(UserMute);
    }
    public void OnVoteButton(int id)
    {
        Debug.Log($"[GameManager] Vote Request : {CurrentRoom.Players[id].NickName}");
        SendEventToMaster(VrMafiaEventCode.VoteReq, id);
    }
    public void OnFinalVoteButton(bool pros)
    {
        Debug.Log($"[GameManager] Final Vote Request : {pros}");
        SendEventToMaster(VrMafiaEventCode.FinalVoteReq, pros);
    }
    public void UpdateMission(string text)
    {
        uiManager.SetMissionText(text);
    }
    public void OnMissionButton()
    {
        if (3f < Vector3.Distance(missionManager.missionObject.transform.position, localCharacter.transform.position))
            return;

        uiManager.OnMissionStarted();
    }
    #endregion

    #endregion
}
