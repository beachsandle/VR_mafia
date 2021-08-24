using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;

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
    private Transform[] spawnPositions;
    private Vector3 spawnPosition;
    private PlayerController localPlayerController;
    private RaiseEventOptions eventOption = new RaiseEventOptions() { Receivers = ReceiverGroup.MasterClient };
    #endregion

    #region callback
    public event Action<bool, int[]> GameStarted;
    public event Action DayStarted;
    public event Action NightStarted;
    public event Action<float> VotingStarted;
    public event Action VoteFailed;
    public event Action<int, int[]> VotingEnded;
    public event Action DefenseStarted;
    public event Action FinalVotingStarted;
    public event Action FinalVotingEnded;
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
        int idx = Array.IndexOf(PhotonNetwork.PlayerList, PhotonNetwork.LocalPlayer);
        spawnPosition = spawnPositions[idx + 1].position;
        var player = PhotonNetwork.Instantiate("Player_SE", spawnPosition, Quaternion.identity);
        localPlayerController = player.AddComponent<PlayerController>();
        localPlayerController.SetCamera(Camera.main);
    }
    #endregion

    #region public
    public void ReturnSpawnPosition() => localPlayerController.MoveTo(spawnPosition);
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
            default: break;
        }
    }
    private void OnGameStarted(EventData e)
    {
        var content = (Hashtable)e.CustomData;
        var isMafia = (bool)content["isMafia"];
        var mafiaIds = isMafia ? (int[])content["mafiaIds"] : null;
        Debug.Log($"[GameManager] Set Mafia : {isMafia}");
        GameStarted?.Invoke(isMafia, mafiaIds);
    }
    private void OnDayStarted()
    {
        Debug.Log($"[GameManager] Day Start");
        DayStarted?.Invoke();
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
    #endregion

    #region button handler
    public void OnVoteButton(int num)
    {
        var id = PhotonNetwork.PlayerList[num].ActorNumber;
        Debug.Log($"[GameManager] Vote Request : {id}");
        PhotonNetwork.RaiseEvent((byte)VrMafiaEventCode.VoteReq, id, eventOption, SendOptions.SendReliable);
    }
    public void OnKillButton(int id) { }
    public void OnDeadReportButton(int id) { }
    public void OnFinalVoteButton(bool pros)
    {

    }
    #endregion

    #endregion
}
