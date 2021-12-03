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
    public bool hostMafia = false;
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
    private int[][] missionIdx;
    private readonly HashSet<int> voters = new HashSet<int>();


    private Dictionary<int, Player> players => CurrentRoom.Players;
    private bool finalVotingResult => pros * 2 >= maxVoters;
    private bool isGameEnd => liveMafias == 0 || liveMafias * 2 >= lives;
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
        missionIdx = new int[10][];
        for (int i = 0; i < 10; ++i)
            missionIdx[i] = new int[3];
        var random = new System.Random();
        for (int i = 0; i < 3; ++i)
        {
            var index = Enumerable.Range(0, 10).ToArray();
            for (int j = 0; j < 100; ++j)
            {
                int a = random.Next(0, 10);
                int b = random.Next(0, 10);
                int tmp = index[a];
                index[a] = index[b];
                index[b] = tmp;
            }
            for (int j = 0; j < 10; ++j)
                missionIdx[j][i] = index[j];
        }
        for (int i = 0; i < 10; ++i)
            missionIdx[i][2] /= 2;
    }
    private void SelectMafia()
    {
        if (hostMafia)
        {
            mafiaIds = new int[] { LocalPlayer.ActorNumber };
        }
        else
        {
            mafiaIds = (from p in players.Values
                        select p.ActorNumber).OrderBy(a => Guid.NewGuid())
                       .Take(players.Count > 6 ? 2 : 1)
                       .ToArray();
        }
        liveMafias = mafiaIds.Length;
    }
    private void SetRole()
    {
        var citizen = from p in players.Values where !mafiaIds.Contains(p.ActorNumber) select p.ActorNumber;
        MulticastEvent(VrMafiaEventCode.GameStart, mafiaIds, new Hashtable() { { "isMafia", true }, { "mafiaIds", mafiaIds } });
        MulticastEvent(VrMafiaEventCode.GameStart, citizen.ToArray(), new Hashtable() { { "isMafia", false } });
        foreach (var c in citizen)
        {
            UnicastEvent(VrMafiaEventCode.SetMission, c, missionIdx[c - 1]);
        }
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
        UnicastEvent(VrMafiaEventCode.KillReady, id);
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
        foreach (var id in players.Where(p => p.Value.GetAlive()).Select(p => p.Key))
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
        BroadcastEvent(VrMafiaEventCode.DayStart);
        deadId = -1;
        Invoke(nameof(NightStart), dayTime);
    }
    private void NightStart()
    {
        CancelInvoke(nameof(NightStart));
        if (phase != GamePhase.Day) return;
        Debug.Log($"[GameServer] Night Start");
        phase = GamePhase.Night;
        BroadcastEvent(VrMafiaEventCode.NightStart, deadId);
        Invoke(nameof(VotingStart), nightTime);
    }
    private void VotingStart()
    {
        CancelInvoke(nameof(VotingStart));
        if (phase != GamePhase.Night) return;
        Debug.Log($"[GameServer] Voting Start");
        phase = GamePhase.Voting;
        InitVotingPhase();
        BroadcastEvent(VrMafiaEventCode.VotingStart, votingTime);
        Invoke(nameof(VotingEnd), votingTime);
    }
    private void VotingEnd()
    {
        CancelInvoke(nameof(VotingEnd));
        if (phase != GamePhase.Voting) return;
        Debug.Log($"[GameServer] Voting End");
        phase = GamePhase.Voting_End;
        GetElectedPlayer();
        BroadcastEvent(VrMafiaEventCode.VotingEnd, new Hashtable() { { "electedId", electedId }, { "result", voteCounts } });
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
        BroadcastEvent(VrMafiaEventCode.DefenseStart, new Hashtable() { { "electedId", electedId }, { "defenseTime", defenseTime } });
        Invoke(nameof(FinalVotingStart), defenseTime);
    }
    private void FinalVotingStart()
    {
        CancelInvoke(nameof(FinalVotingStart));
        if (phase != GamePhase.Defense) return;
        Debug.Log($"[GameServer] Final Voting Start");
        phase = GamePhase.FinalVoting;
        ResetVoters();
        BroadcastEvent(VrMafiaEventCode.FinalVotingStart, finalVotingTime);
        Invoke(nameof(FinalVotingEnd), finalVotingTime);
    }
    private void FinalVotingEnd()
    {
        CancelInvoke(nameof(FinalVotingEnd));
        if (phase != GamePhase.FinalVoting) return;
        Debug.Log($"[GameServer] Final Voting End");
        phase = GamePhase.FinalVoting_End;

        BroadcastEvent(VrMafiaEventCode.FinalVotingEnd, new Hashtable() { { "electedId", finalVotingResult ? electedId : -1 }, { "pros", pros } });
        if (finalVotingResult)
        {
            PlayerDead(electedId);
        }
        Invoke(nameof(DayStart), votingResultTime);
    }
    private void GameEnd()
    {
        if (!checkGameEnd && lives > 1)
            return;
        phase = GamePhase.GameEnd;
        var mafiaWin = liveMafias != 0;
        BroadcastEvent(VrMafiaEventCode.GameEnd, new Hashtable() { { "mafiaWin", mafiaWin }, { "mafiaIds", mafiaIds } });
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
            case VrMafiaEventCode.MissionComplete:
                OnMissionComplete(photonEvent);
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
            !players[data.Sender].GetAlive() ||
            !players.ContainsKey(targetId) ||
            !players[targetId].GetAlive() ||
            mafiaIds.Contains(targetId))
            cool = -1;
        else
        {
            PlayerDead(targetId);
            BroadcastEvent(VrMafiaEventCode.DieEvent, new Hashtable() { { "killerId", data.Sender }, { "targetId", targetId } });
            StartCoroutine(KillReady(data.Sender));
        }
        Debug.Log($"[GameServer] On Kill Request : {players[data.Sender].NickName} -> {players[targetId].NickName}, Result : {cool}");
        UnicastEvent(VrMafiaEventCode.KillRes, data.Sender, cool);
    }
    private void OnDeadReport(EventData data)
    {
        var targetId = (int)data.CustomData;
        if (phase != GamePhase.Day ||
            !players[data.Sender].GetAlive() ||
            !players.ContainsKey(targetId) ||
            players[targetId].GetAlive())
            return;
        deadId = targetId;
        NightStart();
        Debug.Log($"[GameServer] On Dead Report : {players[data.Sender].NickName} -> {players[targetId].NickName}");
    }
    private void OnVoteRequest(EventData data)
    {
        bool result = true;
        int targetId = (int)data.CustomData;
        if (phase != GamePhase.Voting || !voters.Contains(data.Sender) || !players.ContainsKey(targetId) || !players[targetId].GetAlive())
            result = false;
        else
        {
            voters.Remove(data.Sender);
            ++voteCounts[Array.IndexOf(PlayerList, players[targetId])];
        }
        Debug.Log($"[GameServer] On Vote Request : {players[data.Sender].NickName} -> {players[targetId].NickName}, result : {result}");
        UnicastEvent(VrMafiaEventCode.VoteRes, data.Sender, result);
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
        UnicastEvent(VrMafiaEventCode.FinalVoteRes, data.Sender, result);
        if (voters.Count == 0)
            FinalVotingEnd();
    }
    private void OnMissionComplete(EventData data)
    {
        Debug.Log($"[GameServer] OnMissionComplete : {players[data.Sender].NickName}");
        BroadcastEvent(VrMafiaEventCode.MissionEvent, players[data.Sender].NickName);
    }
    #endregion
}
