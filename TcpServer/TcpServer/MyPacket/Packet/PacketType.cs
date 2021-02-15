using System;
using System.Collections.Generic;
using System.Text;

namespace MyPacket
{
    /// <summary>
    /// 패킷의 종류를 나타내는 열거형
    /// </summary>
    [Serializable]
    enum PacketType
    {
        NONE,
        CONNECT,
        DISCONNECT,
        SET_NAME,
        ROOM_LIST_REQ,
        MOVE,
        END
    }
}
