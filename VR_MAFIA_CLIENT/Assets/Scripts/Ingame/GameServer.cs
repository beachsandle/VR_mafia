using ExitGames.Client.Photon;
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
    private int[] voteCounts = new int[10];

    private Player[] players => PhotonNetwork.PlayerList;
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
    private void GameStart()
    {
        Debug.Log($"[GameServer] Game Start");
        SelectMafia();
        SetRole();
        FirstDayStart();
    }
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
        mafiaIds = (from p in players
                    select p.ActorNumber).OrderBy(a => Guid.NewGuid())
                   .Take(players.Length > 6 ? 2 : 1)
                   .ToArray();
    }
    private void SetRole()
    {
        var citizen = from p in players where !mafiaIds.Contains(p.ActorNumber) select p.ActorNumber;
        SendUnicastEvent(VrMafiaEventCode.GameStart, mafiaIds, new Hashtable() { { "isMafia", true }, { "mafiaIds", mafiaIds } });
        SendUnicastEvent(VrMafiaEventCode.GameStart, citizen.ToArray(), new Hashtable() { { "isMafia", false } });
    }

    #region phase change
    private void FirstDayStart()
    {
        if (phase == GamePhase.Day) return;
        Debug.Log($"[GameServer] First Day Start");
        phase = GamePhase.Day;
        Invoke("NightStart", dayTime);
    }
    private void DayStart()
    {
        if (phase == GamePhase.Day) return;
        Debug.Log($"[GameServer] Day Start");
        phase = GamePhase.Day;
        SendBroadcastEvent(VrMafiaEventCode.DayStart);
        Invoke("NightStart", dayTime);
    }
    private void NightStart()
    {
        if (phase == GamePhase.Night) return;
        Debug.Log($"[GameServer] Night Start");
        phase = GamePhase.Night;
        SendBroadcastEvent(VrMafiaEventCode.NightStart);
        Invoke("VotingStart", nightTime);
    }
    private void VotingStart()
    {
        if (phase == GamePhase.Voting) return;
        Debug.Log($"[GameServer] Voting Start");
        phase = GamePhase.Voting;
        electedId = -1;
        SendBroadcastEvent(VrMafiaEventCode.VotingStart, votingTime);
        Invoke("VotingEnd", votingTime);
    }
    private void VotingEnd()
    {
        if (phase == GamePhase.Voting_End) return;
        Debug.Log($"[GameServer] Voting End");
        phase = GamePhase.Voting_End;
        SendBroadcastEvent(VrMafiaEventCode.VotingEnd);
        Invoke("DayStart", votingResultTime);
    }
    private void DefenseStart() { }
    private void FinalVotingStart() { }
    private void FinalVotingEnd() { }
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
            default: break;
        }
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);
    }
    private void OnKillRequest(EventData data)
    {
        int targetId = (int)data.CustomData;
        Debug.Log($"[GameServer] On Kill Request : {players[data.Sender].NickName} -> {players[targetId]}");
        SendUnicastEvent(VrMafiaEventCode.KillRes, new int[] { data.Sender }, true);
        SendBroadcastEvent(VrMafiaEventCode.DieEvent, targetId);
    }
    private void OnVoteRequest(EventData data)
    {
        int targetId = (int)data.CustomData;
        Debug.Log($"[GameServer] On Vote Request : {players[data.Sender].NickName} -> {players[targetId]}");
        SendUnicastEvent(VrMafiaEventCode.VoteRes, new int[] { data.Sender }, true);
    }
    private void OnReceiveDeadReport()
    {

    }
    #endregion
}
