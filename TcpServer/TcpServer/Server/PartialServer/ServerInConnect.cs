using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyPacket
{
    public partial class GameServer
    {
        private void EnrollConnectHandler(User user)
        {
            user.On(PacketType.CONNECT, OnConnect);
            user.On(PacketType.DISCONNECT, OnDisconnect);
        }
        private void OnConnect(MySocket socket, Packet packet)
        {
            (socket as User).Connect();
        }
        private void OnDisconnect(MySocket socket, Packet packet)
        {
            (socket as User).Disconnect();
        }
    }
}
