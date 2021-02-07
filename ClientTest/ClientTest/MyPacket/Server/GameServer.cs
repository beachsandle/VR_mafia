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
        private List<User> users = new List<User>();
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
                users.Add(user);
                user.On(PacketType.CONNECT, OnConnect);
                user.On(PacketType.DISCONNECT, OnDisconnect);
                user.On(PacketType.SET_NAME, OnSetName);
                user.Listen();
            }
        }
        private void OnConnect(MySocket socket, Packet packet)
        {
            var user = socket as User;
            Console.WriteLine("connect");
            user.Emit(PacketType.CONNECT, new ConnectData(user.ID).ToBytes());
            user.Emit(PacketType.SET_NAME, new SetNameData("민수").ToBytes());
        }
        private void OnDisconnect(MySocket socket, Packet packet)
        {
            Console.WriteLine("disconnect");
            socket.Close();
        }
        private void OnSetName(MySocket socket, Packet packet)
        {
            var user = socket as User;
            var data = new SetNameData();
            data.FromBytes(packet.Bytes);
            Console.WriteLine($"set name : {data.UserName}");
            user.Name = data.UserName;
        }
    }
}
