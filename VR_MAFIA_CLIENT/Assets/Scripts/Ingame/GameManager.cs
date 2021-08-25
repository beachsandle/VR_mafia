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
    private Player target;
    private Transform[] spawnPositions;
    private Vector3 spawnPosition;
    private PlayerController localPlayerController;
    private readonly Dictionary<int, PlayerCharacter> playerObjs = new Dictionary<int, PlayerCharacter>();
    private readonly RaiseEventOptions eventOption = new RaiseEventOptions() { Receivers = ReceiverGroup.MasterClient };
    #endregion

    #region property
    public bool IsMafia { get; private set; }
    public bool MenuOpened { get; set; } = false;
    public bool PhaseChanging { get; set; } = false;
    public bool IsVoting { get; private set; } = false;
    #endregion

    #region callback

    #region ui callback
    public event Action<bool> FoundTarget;
    #endregion

    #region server callback
    public event Action<bool, int[]> GameStarted;
    public event Action DayStarted;
    public event Action NightStarted;
    public event Action<float> VotingStarted;
    public event Action VoteFailed;
    public event Action<int, int[]> VotingEnded;
    public event Action<float> DefenseStarted;
    public event Action<float> FinalVotingStarted;
    public event Action FinalVoteFailed;
    public event Action<bool, int> FinalVotingEnded;
    #endregion

    #endregion

    #region unity message
    private void Awake()
    {
        FindReference();
        Init();
    }
    #endregion

    #region method

    #region private
    private void FindReference()
    {
        spawnPositions = transform.Find("SpawnPosition").GetComponentsInChildren<Transform>();
    }
    private void Init()
    {
        if (PhotonManager.Instance != null)
            SpawnPlayer();
    }
    private void SpawnPlayer()
    {
        int idx = Array.IndexOf(PlayerList, LocalPlayer);
        spawnPosition = spawnPositions[idx + 1].position;
        var player = PhotonNetwork.Instantiate("Player_SE", spawnPosition, Quaternion.identity);
        localPlayerController = player.AddComponent<PlayerController>();
        localPlayerController.SetCamera(Camera.main);
    }
    #endregion

    #region public
    public void ReturnSpawnPosition()
    {
        if (LocalPlayer.Alive())
            localPlayerController.MoveTo(spawnPosition);
    }
    #endregion

    #endregion

    #region event handler

    #region message handler
    public void OnEvent(EventData photonEvent)
    {
        switch ((VrMafiaEventCode)photonEvent.Code)
        {
            case VrMafiaEventCode.GameStart:
                OnGameStarted(photonEvent);
                break;
            case VrMafiaEventCode.DayStart:
                OnDayStarted();
                break;
            case VrMafiaEventCode.DieEvent:
                OnDieEvent(photonEvent);
                break;
            case VrMafiaEventCode.NightStart:
                OnNightStarted();
                break;
            case VrMafiaEventCode.VotingStart:
                OnVotingStarted(photonEvent);
                break;
            case VrMafiaEventCode.VoteRes:
                OnVoteResponse(photonEvent);
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
            case VrMafiaEventCode.FinalVoteRes:
                OnFinalVoteResponse(photonEvent);
                break;
            case VrMafiaEventCode.FinalVotingEnd:
                OnFinalVotingEnded(photonEvent);
                break;
            default: break;
        }
    }
    private void OnGameStarted(EventData data)
    {
        var content = (Hashtable)data.CustomData;
        IsMafia = (bool)content["isMafia"];
        var mafiaIds = IsMafia ? (int[])content["mafiaIds"] : null;
        Debug.Log($"[GameManager] Game Start, Is Mafia : {IsMafia}");
        GameStarted?.Invoke(IsMafia, mafiaIds);
    }
    private void OnDayStarted()
    {
        Debug.Log($"[GameManager] Day Start");
        IsVoting = false;
        DayStarted?.Invoke();
    }
    private void OnDieEvent(EventData data)
    {
        playerObjs[(int)data.CustomData].Die();
        Debug.Log($"[GameManager] Die Event : {CurrentRoom.Players[(int)data.CustomData].NickName}");
    }
    private void OnNightStarted()
    {
        Debug.Log($"[GameManager] Night Start");
        NightStarted?.Invoke();
    }
    private void OnVotingStarted(EventData data)
    {
        var votingTime = (float)data.CustomData;
        Debug.Log($"[GameManager] Voting Start : {votingTime}");
        IsVoting = true;
        VotingStarted?.Invoke(votingTime);
    }
    private void OnVoteResponse(EventData data)
    {
        var result = (bool)data.CustomData;
        Debug.Log($"[GameManager] Vote Response : {result}");
        if (!result)
            VoteFailed?.Invoke();
    }
    private void OnVotingEnded(EventData data)
    {
        var content = (Hashtable)data.CustomData;
        var electedId = (int)content["electedId"];
        var result = (int[])content["result"];
        Debug.Log($"[GameManager] elected : {electedId}, result : {string.Join(" ", result)}");
        VotingEnded?.Invoke(electedId, result);
    }
    private void OnFinalVotingStarted(EventData data)
    {
        var finalVotingTime = (float)data.CustomData;
        Debug.Log($"[GameManager] Final Voting Start : {finalVotingTime}");
        FinalVotingStarted?.Invoke(finalVotingTime);
    }
    private void OnDefenseStarted(EventData data)
    {
        var defenseTime = (float)data.CustomData;
        Debug.Log($"[GameManager] Defense Start : {defenseTime}");
        DefenseStarted?.Invoke(defenseTime);
    }
    private void OnFinalVoteResponse(EventData data)
    {
        var result = (bool)data.CustomData;
        Debug.Log($"[GameManager] Final Vote Response : {result}");
        if (!result)
            FinalVoteFailed?.Invoke();
    }
    private void OnFinalVotingEnded(EventData data)
    {
        var content = (Hashtable)data.CustomData;
        var result = (bool)content["result"];
        var pros = (int)content["pros"];
        Debug.Log($"[GameManager] result : {result}, pros : {pros}");
        FinalVotingEnded?.Invoke(result, pros);
    }
    #endregion

    #region ui handler
    public void OnSpwanPlayer(PlayerCharacter obj)
    {
        playerObjs[obj.Owner.ActorNumber] = obj;
    }
    public void OnFoundTarget(Player p)
    {
        if (p != null && p.Alive())
            target = p;
        else
            target = null;
        FoundTarget?.Invoke(target != null);
    }
    public void OnKillButton()
    {
        if (target == null ||
            !target.Alive() ||
            !LocalPlayer.Alive() ||
            !IsMafia)
            return;
        Debug.Log($"[GameManager] Kill Request : {target.NickName}");
        RaiseEvent((byte)VrMafiaEventCode.KillReq, target.ActorNumber, eventOption, SendOptions.SendReliable);
    }
    public void OnDeadReportButton() { }
    public void OnVoteButton(int num)
    {
        var id = PlayerList[num].ActorNumber;
        Debug.Log($"[GameManager] Vote Request : {PlayerList[num].NickName}");
        RaiseEvent((byte)VrMafiaEventCode.VoteReq, id, eventOption, SendOptions.SendReliable);
    }
    public void OnFinalVoteButton(bool pros)
    {
        Debug.Log($"[GameManager] Final Vote Request : {pros}");
        RaiseEvent((byte)VrMafiaEventCode.FinalVoteReq, pros, eventOption, SendOptions.SendReliable);
    }
    #endregion

    #endregion
}
