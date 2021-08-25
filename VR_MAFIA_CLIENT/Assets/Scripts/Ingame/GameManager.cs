﻿using ExitGames.Client.Photon;
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
    [SerializeField] private InGameUIManager uiManager;
    private Player target;
    private Transform[] spawnPositions;
    private Vector3 spawnPosition;
    private PlayerCharacter localCharacter;
    private readonly Dictionary<int, PlayerCharacter> playerObjs = new Dictionary<int, PlayerCharacter>();
    private readonly RaiseEventOptions eventOption = new RaiseEventOptions() { Receivers = ReceiverGroup.MasterClient };
    #endregion

    #region property
    public bool IsMafia { get; private set; }
    public bool MenuOpened { get; set; } = false;
    public bool PhaseChanging { get; set; } = false;
    public bool IsVoting { get; private set; } = false;
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
        localCharacter = PhotonNetwork.Instantiate("Player_SE", spawnPosition, Quaternion.identity).GetComponent<PlayerCharacter>();
    }
    #endregion

    #region public
    public void ReturnSpawnPosition()
    {
        if (LocalPlayer.Alive())
            localCharacter.Controller.MoveTo(spawnPosition);
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
            case VrMafiaEventCode.KillRes:
                OnKillResponse(photonEvent);
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
            case VrMafiaEventCode.GameEnd:
                OnGameEnded(photonEvent);
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
        uiManager.OnGameStarted(IsMafia, mafiaIds);
    }
    private void OnDayStarted()
    {
        Debug.Log($"[GameManager] Day Start");
        IsVoting = false;
        uiManager.OnDayStarted();
    }
    private void OnKillResponse(EventData photonEvent)
    {
        var result = (bool)photonEvent.CustomData;
        Debug.Log($"[GameManager] Kill Response : {result}");
        if (!result)
            //Todo: 킬 실패 만들어야함
            uiManager.OnKillFailed();
    }

    private void OnDieEvent(EventData data)
    {
        playerObjs[(int)data.CustomData].Die();
        Debug.Log($"[GameManager] Die Event : {CurrentRoom.Players[(int)data.CustomData].NickName}");
    }
    private void OnNightStarted()
    {
        Debug.Log($"[GameManager] Night Start");
        uiManager.OnNightStarted();
    }
    private void OnVotingStarted(EventData data)
    {
        var votingTime = (float)data.CustomData;
        Debug.Log($"[GameManager] Voting Start : {votingTime}");
        IsVoting = true;
        uiManager.OnVotingStarted(votingTime);
    }
    private void OnVoteResponse(EventData data)
    {
        var result = (bool)data.CustomData;
        Debug.Log($"[GameManager] Vote Response : {result}");
        if (!result)
            uiManager.OnVoteFailed();
    }
    private void OnVotingEnded(EventData data)
    {
        var content = (Hashtable)data.CustomData;
        var electedId = (int)content["electedId"];
        var result = (int[])content["result"];
        Debug.Log($"[GameManager] elected : {electedId}, result : {string.Join(" ", result)}");
        uiManager.OnVotingEnded(electedId, result);
    }
    private void OnFinalVotingStarted(EventData data)
    {
        var finalVotingTime = (float)data.CustomData;
        Debug.Log($"[GameManager] Final Voting Start : {finalVotingTime}");
        uiManager.OnFinalVotingStarted(finalVotingTime);
    }
    private void OnDefenseStarted(EventData data)
    {
        var defenseTime = (float)data.CustomData;
        Debug.Log($"[GameManager] Defense Start : {defenseTime}");
        uiManager.OnDefenseStarted(defenseTime);
    }
    private void OnFinalVoteResponse(EventData data)
    {
        var result = (bool)data.CustomData;
        Debug.Log($"[GameManager] Final Vote Response : {result}");
        if (!result)
            uiManager.OnFinalVoteFailed();
    }
    private void OnFinalVotingEnded(EventData data)
    {
        var content = (Hashtable)data.CustomData;
        var result = (bool)content["result"];
        var pros = (int)content["pros"];
        Debug.Log($"[GameManager] result : {result}, pros : {pros}");
        uiManager.OnFinalVotingEnded(result, pros);
    }
    private void OnGameEnded(EventData data)
    {
        var content = (Hashtable)data.CustomData;
        var mafiaWin = (bool)content["mafiaWin"];
        var mafiaIds = (int[])content["mafiaIds"];
        Debug.Log($"[GameManager] Game End, Mafia Win : {mafiaWin}");
        uiManager.OnGameEnded(mafiaWin, mafiaIds);
    }
    #endregion

    #region ui handler
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        playerObjs.Remove(otherPlayer.ActorNumber);
    }
    public void OnSpwanPlayer(PlayerCharacter obj)
    {
        playerObjs[obj.Owner.ActorNumber] = obj;
    }
    public void OnFoundTarget(Player p)
    {
        target = p;
        uiManager.OnFoundTarget(p);
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
    public void OnVoteButton(int id)
    {
        Debug.Log($"[GameManager] Vote Request : {PlayerList[id].NickName}");
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
