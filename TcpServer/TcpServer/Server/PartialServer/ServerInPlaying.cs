using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyPacket
{
    public partial class GameServer
    {
        private void EnrollPlayingHandler(User user)
        {
            for (PacketType type = PacketType.INGAME_PACKET + 1; type < PacketType.END; ++type)
            {
                user.On(type, IngameHandler);
            }
        }
        private void IngameHandler(MySocket socket, Packet packet)
        {
            var user = socket as User;
            user.Room.Enqueue(user.Id, packet);
        }
    }
}
