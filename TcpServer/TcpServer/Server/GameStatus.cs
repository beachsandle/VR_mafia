using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyPacket
{
    public enum GameStatus
    {
        NONE,
        CONNECT,
        LOBBY,
        WAITTING,
        DAY,
        NIGHT,
        VOTE,
        VOTE_RESULT,
        DEFENSE,
        FINAL_VOTE,
        END
    }
}
