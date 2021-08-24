﻿using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class GameServer : MonoBehaviourPunCallbacks, IOnEventCallback
{
    #region field
    public float dayTime = 30f;
    public float nightTime = 30f;
    public float votingTime = 15f;
    public float votingResultTime = 3f;
    public float defenseTime = 15f;
    public float finalVotingTime = 15f;
    private GamePhase phase = GamePhase.None;
    private RaiseEventOptions broadcastOption = new RaiseEventOptions() { Receivers = ReceiverGroup.All };
    private RaiseEventOptions unicastOption = new RaiseEventOptions() { Receivers = ReceiverGroup.Others };

    private int[] mafiaIds;

    private int electedId;
    private int[] voteCounts;
    private int pros;
    private int maxVoters;
    private HashSet<int> voters = new HashSet<int>();

    private Dictionary<int, Player> players => PhotonNetwork.CurrentRoom.Players;
    #endregion

    #region unity message
    private void Start()
    {
        if (PhotonNetwork.IsMasterClient)
            GameStart();
        else
            Destroy(GetComponent<GameServer>());
    }
    #endregion

    #region method
    private void SendBroadcastEvent(VrMafiaEventCode code, object content = null)
    {
        PhotonNetwork.RaiseEvent((byte)code, content, broadcastOption, SendOptions.SendReliable);
    }
    private void SendUnicastEvent(VrMafiaEventCode code, int[] targets, object content = null)
    {
        unicastOption.TargetActors = targets;
        PhotonNetwork.RaiseEvent((byte)code, content, unicastOption, SendOptions.SendReliable);
    }
    private void SelectMafia()
    {
        mafiaIds = (from p in players.Values
                    select p.ActorNumber).OrderBy(a => Guid.NewGuid())
                   .Take(players.Count > 6 ? 2 : 1)
                   .ToArray();
    }
    private void SetRole()
    {
        var citizen = from p in players.Values where !mafiaIds.Contains(p.ActorNumber) select p.ActorNumber;
        SendUnicastEvent(VrMafiaEventCode.GameStart, mafiaIds, new Hashtable() { { "isMafia", true }, { "mafiaIds", mafiaIds } });
        SendUnicastEvent(VrMafiaEventCode.GameStart, citizen.ToArray(), new Hashtable() { { "isMafia", false } });
    }
    private void InitVotingPhase()
    {
        electedId = -1;
        pros = 0;
        voteCounts = new int[10];
        ResetVoters();
        maxVoters = voters.Count;
    }
    private void ResetVoters()
    {
        voters.Clear();
        foreach (var id in players.Where(p => p.Value.Alive()).Select(p => p.Key))
            voters.Add(id);
    }
    private void GetElectedPlayer()
    {
        var result = from i in Enumerable.Range(0, PhotonNetwork.PlayerList.Length)
                     orderby voteCounts[i] descending
                     select (i, voteCounts[i]);
        electedId = PhotonNetwork.PlayerList[result.First().i].ActorNumber;
        //최대 득표수가 1이거나 동수일 경우
        if (result.First().Item2 < 2 ||
            result.First().Item2 == result.ElementAt(1).Item2)
            electedId = -1;
    }
    private bool JudgeFinalVoting()
    {
        return pros * 2 >= maxVoters;
    }

    #region phase change
    private void GameStart()
    {
        Debug.Log($"[GameServer] Game Start");
        SelectMafia();
        SetRole();
        FirstDayStart();
    }
    private void FirstDayStart()
    {
        if (phase == GamePhase.Day) return;
        Debug.Log($"[GameServer] First Day Start");
        phase = GamePhase.Day;
        Invoke("NightStart", dayTime);
    }
    private void DayStart()
    {
        CancelInvoke("DayStart");
        if (phase != GamePhase.Voting_End && phase != GamePhase.FinalVoting_End) return;
        Debug.Log($"[GameServer] Day Start");
        phase = GamePhase.Day;
        SendBroadcastEvent(VrMafiaEventCode.DayStart);
        Invoke("NightStart", dayTime);
    }
    private void NightStart()
    {
        CancelInvoke("NightStart");
        if (phase != GamePhase.Day) return;
        Debug.Log($"[GameServer] Night Start");
        phase = GamePhase.Night;
        SendBroadcastEvent(VrMafiaEventCode.NightStart);
        Invoke("VotingStart", nightTime);
    }
    private void VotingStart()
    {
        CancelInvoke("VotingStart");
        if (phase != GamePhase.Night) return;
        Debug.Log($"[GameServer] Voting Start");
        phase = GamePhase.Voting;
        InitVotingPhase();
        SendBroadcastEvent(VrMafiaEventCode.VotingStart, votingTime);
        Invoke("VotingEnd", votingTime);
    }
    private void VotingEnd()
    {
        CancelInvoke("VotingEnd");
        if (phase != GamePhase.Voting) return;
        Debug.Log($"[GameServer] Voting End");
        phase = GamePhase.Voting_End;
        GetElectedPlayer();
        SendBroadcastEvent(VrMafiaEventCode.VotingEnd, new Hashtable() { { "electedId", electedId }, { "result", voteCounts } });
        if (electedId != -1)
            Invoke("DefenseStart", votingResultTime);
        else
            Invoke("DayStart", votingResultTime);
    }
    private void DefenseStart()
    {
        CancelInvoke("DefenseStart");
        if (phase != GamePhase.Voting_End) return;
        Debug.Log($"[GameServer] Defense Start");
        phase = GamePhase.Defense;
        SendBroadcastEvent(VrMafiaEventCode.DefenseStart, defenseTime);
        Invoke("FinalVotingStart", defenseTime);
    }
    private void FinalVotingStart()
    {
        CancelInvoke("FinalVotingStart");
        if (phase != GamePhase.Defense) return;
        Debug.Log($"[GameServer] Final Voting Start");
        phase = GamePhase.FinalVoting;
        ResetVoters();
        SendBroadcastEvent(VrMafiaEventCode.FinalVotingStart, finalVotingTime);
        Invoke("FinalVotingEnd", finalVotingTime);
    }
    private void FinalVotingEnd()
    {
        CancelInvoke("FinalVotingEnd");
        if (phase != GamePhase.FinalVoting) return;
        Debug.Log($"[GameServer] Final Voting End");
        phase = GamePhase.FinalVoting_End;
        var result = JudgeFinalVoting();
        SendBroadcastEvent(VrMafiaEventCode.FinalVotingEnd, new Hashtable() { { "result", result }, { "pros", pros } });
        //Todo: 가결될 경우 추방
        Invoke("DayStart", votingResultTime);
    }
    #endregion

    #endregion

    #region event handler
    public void OnEvent(EventData photonEvent)
    {
        switch ((VrMafiaEventCode)photonEvent.Code)
        {
            case VrMafiaEventCode.VoteReq:
                OnVoteRequest(photonEvent);
                break;
            case VrMafiaEventCode.FinalVoteReq:
                OnFinalVoteRequest(photonEvent);
                break;
            default: break;
        }
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);
    }
    private void OnKillRequest(EventData data)
    {
        bool result = true;
        var targetId = (int)data.CustomData;
        if (phase != GamePhase.Day || !players[data.Sender].Alive() || !players.ContainsKey(targetId) || !players[targetId].Alive())
            result = false;
        else
        {
            players[targetId].Die();
            SendBroadcastEvent(VrMafiaEventCode.DieEvent, targetId);
        }
        Debug.Log($"[GameServer] On Kill Request : {players[data.Sender].NickName} -> {players[targetId].NickName}, Result : {result}");
        SendUnicastEvent(VrMafiaEventCode.KillRes, new int[] { data.Sender }, result);
    }
    private void OnVoteRequest(EventData data)
    {
        bool result = true;
        int targetId = (int)data.CustomData;
        if (phase != GamePhase.Voting || !voters.Contains(data.Sender) || !players.ContainsKey(targetId) || !players[targetId].Alive())
            result = false;
        else
        {
            voters.Remove(data.Sender);
            ++voteCounts[Array.IndexOf(PhotonNetwork.PlayerList, players[targetId])];
        }
        Debug.Log($"[GameServer] On Vote Request : {players[data.Sender].NickName} -> {players[targetId].NickName}, result : {result}");
        SendUnicastEvent(VrMafiaEventCode.VoteRes, new int[] { data.Sender }, result);
        if (voters.Count == 0)
            VotingEnd();
    }
    private void OnFinalVoteRequest(EventData data)
    {
        bool result = true;
        bool pros = (bool)data.CustomData;
        if (phase != GamePhase.FinalVoting || !voters.Contains(data.Sender))
            result = false;
        else
        {
            voters.Remove(data.Sender);
            if (pros)
                ++this.pros;
        }
        Debug.Log($"[GameServer] On Final Vote Request : {players[data.Sender].NickName} -> {pros}, result : {result}");
        SendUnicastEvent(VrMafiaEventCode.FinalVoteRes, new int[] { data.Sender }, result);
        if (voters.Count == 0)
            FinalVotingEnd();
    }
    #endregion
}
