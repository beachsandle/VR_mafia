﻿using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Linq;

namespace MyPacket
{
    public class User : MySocket
    {
        private static int playerId = 1;
        private GameServer server;
        #region property
        public int Id { get; private set; }
        public string Name { get; private set; }
        public GameRoom Room { get; private set; }
        public bool Alive { get; private set; } = true;
        public bool IsMafia { get; private set; } = false;
        public GameStatus Status { get; private set; } = GameStatus.CONNECT;
        #endregion
        #region constructor
        public User(TcpClient client, GameServer server) : base(client)
        {
            Id = playerId++;
            Name = $"Player{Id.ToString("X")}";
            this.server = server;
        }
        #endregion
        #region public method
        public void Connect()
        {
            if (Status == GameStatus.CONNECT)
            {
                Status = GameStatus.LOBBY;
                Emit(PacketType.CONNECT, new ConnectData(Id).ToBytes());
                Emit(PacketType.SET_NAME_RES, new SetNameResData(true, Name).ToBytes());
            }
        }
        public void Disconnect()
        {
            Close();
            switch (Status)
            {
                case GameStatus.WAITTING:
                    Status = GameStatus.NONE;
                    Room.Leave(this);
                    break;
                case GameStatus.DAY:
                case GameStatus.NIGHT:
                case GameStatus.VOTE1:
                case GameStatus.DEFENSE:
                case GameStatus.VOTE2:
                    Room.RemoveUser(Id);
                    break;
            }
            server.RemoveUser(Id);
        }
        public bool SetName(string name)
        {
            var data = new SetNameResData();
            //로비가 아닐 경우 실패
            if (Status != GameStatus.LOBBY)
            {
                data.Result = false;
                Emit(PacketType.SET_NAME_RES, data.ToBytes());
                return false;
            }
            //이름을 변경하고 결과 전송
            Name = name;
            data.UserName = name;
            Emit(PacketType.SET_NAME_RES, data.ToBytes());
            return true;
        }
        public bool SendRoomList(List<GameRoomInfo> roomInfos)
        {
            var data = new RoomListResData();
            if (Status != GameStatus.LOBBY)
            {
                data.Result = false;
                Emit(PacketType.ROOM_LIST_RES, data.ToBytes());
                return false;
            }
            data.Rooms = roomInfos;
            Emit(PacketType.ROOM_LIST_RES, data.ToBytes());
            return true;
        }
        public bool CreateRoom(string roomName)
        {
            var data = new CreateRoomResData();
            if (Status != GameStatus.LOBBY)
            {
                data.Result = false;
                Emit(PacketType.CREATE_ROOM_RES, data.ToBytes());
                return false;
            }
            Room = server.CreateRoom(this, roomName);
            Room.Join(this);
            Status = GameStatus.WAITTING;
            Emit(PacketType.CREATE_ROOM_RES, data.ToBytes());
            return true;
        }
        public bool JoinRoom(GameRoom room)
        {
            var data = new JoinRoomResData();
            if (Status != GameStatus.LOBBY || room == null)
            {
                data.Result = false;
                Emit(PacketType.JOIN_ROOM_RES, data.ToBytes());
                return false;
            }
            if (room.Join(this))
            {
                Room = room;
                data.Users = room.GetUserInfos();
                Status = GameStatus.WAITTING;
                Emit(PacketType.JOIN_ROOM_RES, data.ToBytes());
                return true;
            }
            data.Result = false;
            Emit(PacketType.JOIN_ROOM_RES, data.ToBytes());
            return false;
        }
        public bool LeaveRoom()
        {
            var data = new LeaveResData();
            if (Status != GameStatus.WAITTING || Room == null)
            {
                data.Result = false;
                Emit(PacketType.LEAVE_ROOM_RES, data.ToBytes());
                return false;
            }
            Room = null;
            Status = GameStatus.LOBBY;
            Emit(PacketType.LEAVE_ROOM_RES, data.ToBytes());
            return true;
        }
        public void GameStart(bool isMafia, List<User> team)
        {
            if (Status != GameStatus.WAITTING)
                return;
            var data = new GameStartData(isMafia);
            if (isMafia)
                data.Mafias = (from u in team select u.Id).ToArray();
            Status = GameStatus.DAY;
            Emit(PacketType.GAME_START, data.ToBytes());
        }
        public UserInfo GetInfo()
        {
            return new UserInfo(Id, Name);
        }
        #endregion
    }
}
