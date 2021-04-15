﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Linq;
using MyPacket;

namespace MyPacket
{
    public partial class GameServer
    {
        #region field
        private TcpListener server;
        private Dictionary<int, User> userMap = new Dictionary<int, User>();
        private Dictionary<int, GameRoom> roomMap = new Dictionary<int, GameRoom>();
        #endregion
        #region constructor
        public GameServer(TcpListener server)
        {
            this.server = server;
        }
        #endregion
        #region public method
        //서버를 시작하고 접속한 클라이언트들을 user로 추가
        public void Start()
        {
            server.Start();
            Console.WriteLine("server start");
            while (true)
            {
                var client = server.AcceptTcpClient();
                UserInit(new User(client, this));
            }
        }
        public GameRoom CreateRoom(User user, string roomName)
        {
            var room = new GameRoom(this, user, roomName);
            roomMap[room.Id] = room;
            return room;
        }
        public void RemoveUser(int userId)
        {
            userMap.Remove(userId);
        }
        public void RemoveRoom(int roomId)
        {
            roomMap.Remove(roomId);
        }
        #endregion
        #region private method
        private void UserInit(User user)
        {
            userMap[user.Id] = user;
            EnrollConnectHandler(user);
            EnrollLobbyHandler(user);
            EnrollWatingHandler(user);
            EnrollPlayingHandler(user);
            user.Listen(true);
        }
        private List<GameRoomInfo> GetRoomInfos()
        {
            return (from r in roomMap.Values where !r.IsStarted select r.GetInfo()).ToList();
        }
        #endregion
    }
}
