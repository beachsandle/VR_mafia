using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum VrMafiaEventCode
{
    GAME_START,
    DAY_START,
    NIGHT_START,
    KILL_REQ,
    KILL_RES,
    DIE_EVENT,
    DEAD_REPORT,
    //voting
    START_VOTING,
    VOTE_REQ,
    VOTE_RES,
    VOTE_EVENT,
    VOTING_RESULT,
    START_DEFENSE,
    START_FINAL_VOTING,
    FINAL_VOTE_RES,
    FINAL_VOTE_REQ,
    FINAL_VOTING_RESULT,
    KILL_READY,
    GAME_END
}
