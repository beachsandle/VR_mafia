using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace MyPacket
{
    class User : MySocket
    {
        protected static int playerID = 1;
        public int ID { get; private set; }
        public string Name { get; set; }
        public User(TcpClient client) : base(client)
        {
            ID = playerID++;
            Name = $"Player{ID.ToString("X")}";
        }
    }
}
