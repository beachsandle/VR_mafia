using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum VrMafiaEventCode
{
    /// <summary>
    /// {bool isMafia, int[] mafiaIds}
    /// </summary>
    GameStart,

    /// <summary>
    /// none
    /// </summary>
    DayStart,

    /// <summary>
    /// none
    /// </summary>
    NightStart,

    /// <summary>
    /// int targetId
    /// </summary>
    KillReq,

    /// <summary>
    /// bool result
    /// </summary>
    KillRes,

    /// <summary>
    /// int deadId
    /// </summary>
    DieEvent,

    /// <summary>
    /// 
    /// </summary>
    DeadReport,

    /// <summary>
    /// float votingTime
    /// </summary>
    VotingStart,

    /// <summary>
    /// int targetId
    /// </summary>
    VoteReq,

    /// <summary>
    /// 
    /// </summary>
    VoteRes,

    /// <summary>
    /// 
    /// </summary>
    VoteEvent,

    /// <summary>
    /// 
    /// </summary>
    VotingEnd,

    /// <summary>
    /// 
    /// </summary>
    DefenseStart,

    /// <summary>
    /// 
    /// </summary>
    FinalVotingStart,

    /// <summary>
    /// 
    /// </summary>
    FinalVoteReq,

    /// <summary>
    /// 
    /// </summary>
    FinalVoteRes,

    /// <summary>
    /// 
    /// </summary>
    FinalVotingEnd,

    /// <summary>
    /// 
    /// </summary>
    KillReady,

    /// <summary>
    /// 
    /// </summary>
    GameEnd
}
