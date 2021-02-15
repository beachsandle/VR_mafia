using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using MyPacket;

namespace MyPacket
{
    class GameServer
    {
        private TcpListener server;
        private Dictionary<int, User> users = new Dictionary<int, User>();
        public GameServer(TcpListener server)
        {
            this.server = server;
        }
        /// <summary>
        /// 서버 시작
        /// </summary>
        /// <exception cref="SocketException"></exception>
        public void Start()
        {
            server.Start();
            Console.WriteLine("server start");
            while (true)
            {
                var client = server.AcceptTcpClient();
                var user = new User(client);
                users[user.ID] = user;
                user.On(PacketType.CONNECT, OnConnect);
                user.On(PacketType.DISCONNECT, OnDisconnect);
                user.On(PacketType.SET_NAME, OnSetName);
                user.On(PacketType.MOVE, OnMove);
                user.Listen();
            }
        }
        private void OnConnect(MySocket socket, Packet packet)
        {
            var user = socket as User;
            Console.WriteLine("connect");
            user.Emit(PacketType.CONNECT, new ConnectData(user.ID).ToBytes());
            user.Emit(PacketType.SET_NAME, new SetNameData(user.Name).ToBytes());
        }
        private void OnDisconnect(MySocket socket, Packet packet)
        {
            var user = socket as User;
            Console.WriteLine("disconnect");
            user.Close();
            users.Remove(user.ID);

        }
        private void OnSetName(MySocket socket, Packet packet)
        {
            var user = socket as User;
            var data = new SetNameData();
            data.FromBytes(packet.Bytes);
            Console.WriteLine($"set name : {data.UserName}");
            user.Name = data.UserName;
        }
        private void OnMove(MySocket socket, Packet packet)
        {
            var user = socket as User;
            foreach (var p in users)
            {
                if (p.Value != user)
                {
                    p.Value.Emit(PacketType.MOVE, packet.Bytes);
                }
            }
        }
    }
}
