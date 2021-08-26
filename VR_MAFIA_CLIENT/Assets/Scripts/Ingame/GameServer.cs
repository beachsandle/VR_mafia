using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using Hashtable = ExitGames.Client.Photon.Hashtable;
using static Photon.Pun.PhotonNetwork;
using static PhotonManager;

public class GameServer : MonoBehaviourPunCallbacks, IOnEventCallback
{
    #region field
    public float dayTime = 30f;
    public float nightTime = 30f;
    public float votingTime = 15f;
    public float votingResultTime = 3f;
    public float defenseTime = 15f;
    public float finalVotingTime = 15f;
    public float killCoolTime = 5f;
    public bool checkGameEnd = true;
    private GamePhase phase = GamePhase.None;

    private int[] mafiaIds;
    private int lives = 0;
    private int liveMafias = 0;
    private int electedId;
    private int[] voteCounts;
    private int pros;
    private int maxVoters;
    private int deadId;
    private readonly HashSet<int> voters = new HashSet<int>();


    private Dictionary<int, Player> players => CurrentRoom.Players;
    private bool finalVotingResult => pros * 2 >= maxVoters;
    private bool isGameEnd => checkGameEnd && (liveMafias == 0 || liveMafias * 2 >= lives);
    #endregion

    #region unity message
    private void Start()
    {
        if (IsMasterClient)
            GameStart();
        else
            Destroy(GetComponent<GameServer>());
    }
    #endregion

    #region method

    #region game start
    private void Init()
    {
        lives = PlayerList.Length;
    }
    private void SelectMafia()
    {
        mafiaIds = (from p in players.Values
                    select p.ActorNumber).OrderBy(a => Guid.NewGuid())
                   .Take(players.Count > 6 ? 2 : 1)
                   .ToArray();
        liveMafias = mafiaIds.Length;
    }
    private void SetRole()
    {
        var citizen = from p in players.Values where !mafiaIds.Contains(p.ActorNumber) select p.ActorNumber;
        SendMulticastEvent(VrMafiaEventCode.GameStart, mafiaIds, new Hashtable() { { "isMafia", true }, { "mafiaIds", mafiaIds } });
        SendMulticastEvent(VrMafiaEventCode.GameStart, citizen.ToArray(), new Hashtable() { { "isMafia", false } });
    }
    #endregion

    #region kill
    private void PlayerDead(int id)
    {
        if (players.ContainsKey(id))
        {
            --lives;
            players[id].Die();
            if (mafiaIds.Contains(id))
                --liveMafias;
            if (isGameEnd)
                GameEnd();
        }
    }
    private IEnumerator KillReady(int id)
    {
        yield return new WaitForSeconds(killCoolTime);
        Debug.Log($"[GameServer] Kill Ready : {players[id].NickName}");
        SendUnicastEvent(VrMafiaEventCode.KillReady, id);
    }
    #endregion

    #region voting
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
    #endregion

    #region phase change
    private void GameStart()
    {
        Debug.Log($"[GameServer] Game Start");
        Init();
        SelectMafia();
        SetRole();
        FirstDayStart();
    }
    private void FirstDayStart()
    {
        if (phase == GamePhase.Day) return;
        Debug.Log($"[GameServer] First Day Start");
        phase = GamePhase.Day;
        Invoke(nameof(NightStart), dayTime);
    }
    private void DayStart()
    {
        CancelInvoke(nameof(DayStart));
        if (phase != GamePhase.Voting_End && phase != GamePhase.FinalVoting_End) return;
        Debug.Log($"[GameServer] Day Start");
        phase = GamePhase.Day;
        SendBroadcastEvent(VrMafiaEventCode.DayStart);
        deadId = -1;
        Invoke(nameof(NightStart), dayTime);
    }
    private void NightStart()
    {
        CancelInvoke(nameof(NightStart));
        if (phase != GamePhase.Day) return;
        Debug.Log($"[GameServer] Night Start");
        phase = GamePhase.Night;
        SendBroadcastEvent(VrMafiaEventCode.NightStart, deadId);
        Invoke(nameof(VotingStart), nightTime);
    }
    private void VotingStart()
    {
        CancelInvoke(nameof(VotingStart));
        if (phase != GamePhase.Night) return;
        Debug.Log($"[GameServer] Voting Start");
        phase = GamePhase.Voting;
        InitVotingPhase();
        SendBroadcastEvent(VrMafiaEventCode.VotingStart, votingTime);
        Invoke(nameof(VotingEnd), votingTime);
    }
    private void VotingEnd()
    {
        CancelInvoke(nameof(VotingEnd));
        if (phase != GamePhase.Voting) return;
        Debug.Log($"[GameServer] Voting End");
        phase = GamePhase.Voting_End;
        GetElectedPlayer();
        SendBroadcastEvent(VrMafiaEventCode.VotingEnd, new Hashtable() { { "electedId", electedId }, { "result", voteCounts } });
        if (electedId != -1)
            Invoke(nameof(DefenseStart), votingResultTime);
        else
            Invoke(nameof(DayStart), votingResultTime);
    }
    private void DefenseStart()
    {
        CancelInvoke("DefenseStart");
        if (phase != GamePhase.Voting_End) return;
        Debug.Log($"[GameServer] Defense Start");
        phase = GamePhase.Defense;
        SendBroadcastEvent(VrMafiaEventCode.DefenseStart, new Hashtable() { { "electedId", electedId }, { "defenseTime", defenseTime } });
        Invoke(nameof(FinalVotingStart), defenseTime);
    }
    private void FinalVotingStart()
    {
        CancelInvoke(nameof(FinalVotingStart));
        if (phase != GamePhase.Defense) return;
        Debug.Log($"[GameServer] Final Voting Start");
        phase = GamePhase.FinalVoting;
        ResetVoters();
        SendBroadcastEvent(VrMafiaEventCode.FinalVotingStart, finalVotingTime);
        Invoke(nameof(FinalVotingEnd), finalVotingTime);
    }
    private void FinalVotingEnd()
    {
        CancelInvoke(nameof(FinalVotingEnd));
        if (phase != GamePhase.FinalVoting) return;
        Debug.Log($"[GameServer] Final Voting End");
        phase = GamePhase.FinalVoting_End;

        SendBroadcastEvent(VrMafiaEventCode.FinalVotingEnd, new Hashtable() { { "electedId", finalVotingResult ? electedId : -1 }, { "pros", pros } });
        if (finalVotingResult)
        {
            PlayerDead(electedId);
        }
        Invoke(nameof(DayStart), votingResultTime);
    }
    private void GameEnd()
    {
        phase = GamePhase.GameEnd;
        var mafiaWin = liveMafias != 0;
        SendBroadcastEvent(VrMafiaEventCode.GameEnd, new Hashtable() { { "mafiaWin", mafiaWin }, { "mafiaIds", mafiaIds } });
        Debug.Log($"[GameServer] Game End Mafia {(mafiaWin ? "Win" : "Lose")}");
    }
    #endregion

    #endregion

    #region event handler
    public void OnEvent(EventData photonEvent)
    {
        switch ((VrMafiaEventCode)photonEvent.Code)
        {
            case VrMafiaEventCode.KillReq:
                OnKillRequest(photonEvent);
                break;
            case VrMafiaEventCode.DeadReport:
                OnDeadReport(photonEvent);
                break;
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
        --lives;
        if (mafiaIds.Contains(otherPlayer.ActorNumber))
            --liveMafias;
        if (isGameEnd)
            GameEnd();
    }
    private void OnKillRequest(EventData data)
    {
        var cool = killCoolTime;
        var targetId = (int)data.CustomData;
        if (phase != GamePhase.Day ||
            !players[data.Sender].Alive() ||
            !players.ContainsKey(targetId) ||
            !players[targetId].Alive() ||
            mafiaIds.Contains(targetId))
            cool = -1;
        else
        {
            PlayerDead(targetId);
            SendBroadcastEvent(VrMafiaEventCode.DieEvent, targetId);
            StartCoroutine(KillReady(data.Sender));
        }
        Debug.Log($"[GameServer] On Kill Request : {players[data.Sender].NickName} -> {players[targetId].NickName}, Result : {cool}");
        SendUnicastEvent(VrMafiaEventCode.KillRes, data.Sender, cool);
    }
    private void OnDeadReport(EventData data)
    {
        var targetId = (int)data.CustomData;
        if (phase != GamePhase.Day ||
            !players[data.Sender].Alive() ||
            !players.ContainsKey(targetId) ||
            players[targetId].Alive())
            return;
        deadId = targetId;
        NightStart();
        Debug.Log($"[GameServer] On Dead Report : {players[data.Sender].NickName} -> {players[targetId].NickName}");
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
            ++voteCounts[Array.IndexOf(PlayerList, players[targetId])];
        }
        Debug.Log($"[GameServer] On Vote Request : {players[data.Sender].NickName} -> {players[targetId].NickName}, result : {result}");
        SendUnicastEvent(VrMafiaEventCode.VoteRes, data.Sender, result);
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
        SendUnicastEvent(VrMafiaEventCode.FinalVoteRes, data.Sender, result);
        if (voters.Count == 0)
            FinalVotingEnd();
    }
    #endregion
}
