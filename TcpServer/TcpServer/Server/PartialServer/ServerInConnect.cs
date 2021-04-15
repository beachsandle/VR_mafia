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
            var user = socket as User;
            user.Connect();
            Console.WriteLine($"connect : {user.Id}");
        }
        private void OnDisconnect(MySocket socket, Packet packet)
        {
            var user = socket as User;
            Console.WriteLine($"disconnect : {user.Id}");
            user.LeaveRoom();
            user.Close();
            userMap.Remove(user.Id);
        }
    }
}
