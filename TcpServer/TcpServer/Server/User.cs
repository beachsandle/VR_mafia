using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace MyPacket
{
    class User : MySocket
    {
        private static int playerId = 1;
        private GameRoom room;
        public int Id { get; private set; }
        public string Name { get; set; }
        public User(TcpClient client) : base(client)
        {
            Id = playerId++;
            Name = $"Player{Id.ToString("X")}";
        }
        public bool JoinRoom(GameRoom room)
        {
            this.room = room;
            return room.Join(this);
        }
        public UserInfo GetInfo()
        {
            return new UserInfo(Id, Name);
        }
    }
}
