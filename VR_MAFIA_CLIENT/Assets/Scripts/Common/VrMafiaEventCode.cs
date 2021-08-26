using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum VrMafiaEventCode
{
    #region phase
    /// <summary>
    /// {bool isMafia, int[] mafiaIds}
    /// </summary>
    GameStart,

    /// <summary>
    /// none
    /// </summary>
    DayStart,

    /// <summary>
    /// int deadId(-1 : none)
    /// </summary>
    NightStart,

    /// <summary>
    /// float votingTime
    /// </summary>
    VotingStart,

    /// <summary>
    /// {int electedId, int[] result}
    /// </summary>
    VotingEnd,

    /// <summary>
    /// {int electedId, float defenseTime}
    /// </summary>
    DefenseStart,

    /// <summary>
    /// float finalVotingTime
    /// </summary>
    FinalVotingStart,

    /// <summary>
    /// {int electedId(fail : -1), int pros}
    /// </summary>
    FinalVotingEnd,

    /// <summary>
    /// {bool mafiaWin, int[] mafiaIds}
    /// </summary>
    GameEnd,

    #endregion

    #region event
    /// <summary>
    /// int targetId
    /// </summary>
    KillReq,

    /// <summary>
    /// float coolTime (fail : -1)
    /// </summary>
    KillRes,

    /// <summary>
    /// none
    /// </summary>
    KillReady,

    /// <summary>
    /// int deadId
    /// </summary>
    DieEvent,

    /// <summary>
    /// int targetId
    /// </summary>
    DeadReport,

    /// <summary>
    /// int targetId
    /// </summary>
    VoteReq,

    /// <summary>
    /// bool result
    /// </summary>
    VoteRes,

    /// <summary>
    /// bool pros
    /// </summary>
    FinalVoteReq,

    /// <summary>
    /// bool result
    /// </summary>
    FinalVoteRes,
    #endregion
}
