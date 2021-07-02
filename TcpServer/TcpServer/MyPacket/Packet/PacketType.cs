using System;
using System.Collections.Generic;
using System.Text;

namespace MyPacket
{
    /// <summary>
    /// 패킷의 종류를 나타내는 열거형
    /// </summary>
    [Serializable]
    public enum PacketType
    {
        NONE,
        //connect
        CONNECT,
        DISCONNECT,
        //lobby
        SET_NAME_REQ,
        SET_NAME_RES,
        ROOM_LIST_REQ,
        ROOM_LIST_RES,
        CREATE_ROOM_REQ,
        CREATE_ROOM_RES,
        JOIN_ROOM_REQ,
        JOIN_ROOM_RES,
        JOIN_EVENT,
        //waiting
        LEAVE_ROOM_REQ,
        LEAVE_ROOM_RES,
        LEAVE_EVENT,
        ROOM_DESTROY_EVENT,
        GAME_START_REQ,
        GAME_START,
        //ingame
        INGAME_PACKET,
        PLAYER_LOAD,
        ALL_PLAYER_LOADED,
        DAY_START,
        NIGHT_START,
        MOVE_REQ,
        MOVE_EVENT,
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
        GAME_END,
        //
        END
    }
}
