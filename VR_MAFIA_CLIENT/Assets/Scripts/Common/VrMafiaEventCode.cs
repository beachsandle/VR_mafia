using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum VrMafiaEventCode
{
    /// <summary>
    /// int[] mafiaIds
    /// </summary>
    SetMafia,

    /// <summary>
    /// header only
    /// </summary>
    DayStart,

    /// <summary>
    /// header only
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
    /// int VotingTime
    /// </summary>
    VotingStart,

    /// <summary>
    /// 
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
