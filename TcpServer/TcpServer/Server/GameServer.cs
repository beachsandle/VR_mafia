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
    class GameServer
    {
        private TcpListener server;
        private Dictionary<int, User> users = new Dictionary<int, User>();
        private Dictionary<int, GameRoom> rooms = new Dictionary<int, GameRoom>();
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
                users[user.Id] = user;
                UserInit(user);
            }
        }
        #region connect handler
        private void OnConnect(MySocket socket, Packet packet)
        {
            var user = socket as User;
            Console.WriteLine("connect");
            EnterLobby(user);
            user.Emit(PacketType.CONNECT, new ConnectData(user.Id).ToBytes());
            user.Emit(PacketType.SET_NAME, new SetNameData(user.Name).ToBytes());
        }
        private void OnDisconnect(MySocket socket, Packet packet)
        {
            var user = socket as User;
            Console.WriteLine("disconnect");
            user.Close();
            users.Remove(user.Id);

        }
        #endregion
        private void UserInit(User user)
        {
            user.On(PacketType.CONNECT, OnConnect);
            user.On(PacketType.DISCONNECT, OnDisconnect);
            user.Listen();
        }
        #region lobby handler
        private void OnSetName(MySocket socket, Packet packet)
        {
            var user = socket as User;
            var data = new SetNameData();
            data.FromBytes(packet.Bytes);
            user.Name = data.UserName;
        }
        private void OnRoomListReq(MySocket socket, Packet packet)
        {
            var roominfos = (from r in rooms.Values select r.GetInfo()).ToList();
            socket.Emit(PacketType.ROOM_LIST_RES, new RoomListResData(roominfos).ToBytes());
        }
        private void OnCreateRoomReq(MySocket socket, Packet packet)
        {
            var user = socket as User;
            var data = new CreateRoomReqData();
            data.FromBytes(packet.Bytes);
            var room = new GameRoom(user, data.RoomName);
            rooms[room.Id] = room;
            user.JoinRoom(room);
            OutLobby(user);
            EnterWaitingRoom(user);
            user.Emit(PacketType.CREATE_ROOM_RES);
        }
        private void OnJoinRoomReq(MySocket socket, Packet packet)
        {
            var user = socket as User;
            var data = new JoinRoomReqData();
            data.FromBytes(packet.Bytes);
            var room = rooms[data.RoomId];
            if (user.JoinRoom(room))
            {
                OutLobby(user);
                EnterWaitingRoom(user);
                user.Emit(PacketType.JOIN_ROOM_RES, new JoinRoomResData(true, room.GetUserInfos()).ToBytes());
                room.Broadcast(PacketType.JOIN_EVENT, new JoinEventData(user.GetInfo()).ToBytes(), user);
            }
            else
                user.Emit(PacketType.JOIN_ROOM_RES, new JoinRoomResData(false).ToBytes());

        }
        #endregion
        private void EnterLobby(User user)
        {
            user.Clear(PacketType.CONNECT);
            user.On(PacketType.SET_NAME, OnSetName);
            user.On(PacketType.ROOM_LIST_REQ, OnRoomListReq);
            user.On(PacketType.CREATE_ROOM_REQ, OnCreateRoomReq);
            user.On(PacketType.JOIN_ROOM_REQ, OnJoinRoomReq);

        }
        private void OutLobby(User user)
        {
            user.Clear(PacketType.SET_NAME);
            user.Clear(PacketType.ROOM_LIST_REQ);
            user.Clear(PacketType.CREATE_ROOM_REQ);
            user.Clear(PacketType.JOIN_ROOM_REQ);
        }
        #region waiting handler
        private void OnLeaveRoomReq(MySocket socket, Packet packet)
        {
            var user = socket as User;
            var room = user.Room;
            if (!room.IsStarted)
            {
                user.LeaveRoom();
                OutWaitingRoom(user);
                EnterLobby(user);
                user.Emit(PacketType.LEAVE_ROOM_RES);
                room.Broadcast(PacketType.LEAVE_EVENT, new LeaveEventData(user.Id).ToBytes(), user);
            }
        }
        private void OnGameStartReq(MySocket socket, Packet packet)
        {

        }
        #endregion
        private void EnterWaitingRoom(User user)
        {
            user.On(PacketType.LEAVE_ROOM_REQ, OnLeaveRoomReq);
            user.On(PacketType.GAME_START_REQ, OnGameStartReq);
        }
        private void OutWaitingRoom(User user)
        {
            user.Clear(PacketType.LEAVE_ROOM_REQ);
            user.Clear(PacketType.GAME_START_REQ);
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
