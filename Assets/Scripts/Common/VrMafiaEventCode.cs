using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum VrMafiaEventCode
{
    GameStart,
    DayStart,
    NightStart,
    KillReq,
    KillRes,
    DieEvent,
    DeadReport,
    //voting
    VotingStart,
    VoteReq,
    VoteRes,
    VoteEvent,
    VotingEnd,
    DefenseStart,
    FinalVotingStart,
    FinalVoteReq,
    FinalVoteRes,
    FinalVotingEnd,
    KillReady,
    GameEnd
}
