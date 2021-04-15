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
            user.On(PacketType.MOVE, OnMove);
        }
        private void OnMove(MySocket socket, Packet packet)
        {
            var user = socket as User;
            var room = user.Room;
            room.Broadcast(PacketType.MOVE, packet.Bytes, user);
        }
    }
}
