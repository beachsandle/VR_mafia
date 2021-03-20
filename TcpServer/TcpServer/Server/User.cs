using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace MyPacket
{
    public class User : MySocket
    {
        private static int playerId = 1;
        public int Id { get; private set; }
        public string Name { get; set; }
        public GameRoom Room { get; private set; }
        public User(TcpClient client) : base(client)
        {
            Id = playerId++;
            Name = $"Player{Id.ToString("X")}";
        }
        public bool JoinRoom(GameRoom room)
        {
            Room = room;
            return room.Join(this);
        }
        public void LeaveRoom()
        {
            if (Room != null)
            {
                Room.Leave(this);
                Room = null;
            }
        }
        public UserInfo GetInfo()
        {
            return new UserInfo(Id, Name);
        }
    }
}
