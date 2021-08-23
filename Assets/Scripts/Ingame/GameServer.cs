using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        DayStart();
    }
    private void SendBroadcastEvent(VrMafiaEventCode code, object content = null)
    {
        PhotonNetwork.RaiseEvent((byte)code, content, broadcastOption, SendOptions.SendReliable);
    }

    #region phase change
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
        SendBroadcastEvent(VrMafiaEventCode.VotingStart);
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
    }

    private void OnReceiveDeadReport()
    {

    }
    #endregion
}
