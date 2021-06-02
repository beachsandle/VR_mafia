using System;
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
        private readonly TcpListener server;
        private readonly Dictionary<int, User> userMap = new Dictionary<int, User>();
        private readonly Dictionary<int, GameRoom> roomMap = new Dictionary<int, GameRoom>();
        private readonly Queue<string> logQueue = new Queue<string>();
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
            new Thread(LogThread).Start();
            Log("server start");
            while (true)
            {
                var socket = server.AcceptSocket();
                UserInit(socket);
            }
        }
        public GameRoom CreateRoom(User user, string roomName)
        {
            var room = new GameRoom(this, user, roomName);
            roomMap[room.RoomId] = room;
            return room;
        }
        public GameRoom FindRoomById(int roomId)
        {
            return roomMap.ContainsKey(roomId) ? roomMap[roomId] : null;
        }
        public void RemoveUser(int userId)
        {
            userMap.Remove(userId);
        }
        public void RemoveRoom(int roomId)
        {
            roomMap.Remove(roomId);
        }
        public List<GameRoomInfo> GetRoomInfos()
        {
            return (from r in roomMap.Values
                    where r.Status == GameStatus.WAITTING
                    select r.GetInfo()).ToList();
        }
        public void Log(string log)
        {
            lock (logQueue)
            {
                logQueue.Enqueue(log);
                Monitor.Pulse(logQueue);
            }
        }
        #endregion
        #region private method
        private void UserInit(Socket socket)
        {

            var user = new User(socket, this);
            userMap[user.Id] = user;
            user.Listen(true);
        }
        private void LogThread()
        {
            while (true)
            {
                lock (logQueue)
                {
                    if (logQueue.Count == 0)
                        Monitor.Wait(logQueue);
                    Console.WriteLine(logQueue.Dequeue());
                }
            }
        }
        #endregion
    }
}
