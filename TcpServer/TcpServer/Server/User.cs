using System;
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
        public UserStatus Status { get; private set; } = UserStatus.CONNECT;
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
            if (Status == UserStatus.CONNECT)
            {
                Status = UserStatus.LOBBY;
                Emit(PacketType.CONNECT, new ConnectData(Id).ToBytes());
                Emit(PacketType.SET_NAME_RES, new SetNameResData(true, Name).ToBytes());
            }
        }
        new public void Disconnect()
        {
            Close();
            switch (Status)
            {
                case UserStatus.WAITTING:
                    Status = UserStatus.NONE;
                    Room.Leave(this);
                    break;
                case UserStatus.PLAYING:
                case UserStatus.VOTING:
                    Room.RemoveUser(Id);
                    break;
            }
            server.RemoveUser(Id);
        }
        public bool SetName(string name)
        {
            var data = new SetNameResData();
            //로비가 아닐 경우 실패
            if (Status != UserStatus.LOBBY)
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
            if (Status != UserStatus.LOBBY)
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
            if (Status != UserStatus.LOBBY)
            {
                data.Result = false;
                Emit(PacketType.CREATE_ROOM_RES, data.ToBytes());
                return false;
            }
            Room = server.CreateRoom(this, roomName);
            Room.Join(this);
            Status = UserStatus.WAITTING;
            Emit(PacketType.CREATE_ROOM_RES, data.ToBytes());
            return true;
        }
        public bool JoinRoom(GameRoom room)
        {
            var data = new JoinRoomResData();
            if (Status != UserStatus.LOBBY || room == null)
            {
                data.Result = false;
                Emit(PacketType.JOIN_ROOM_RES, data.ToBytes());
                return false;
            }
            if (room.Join(this))
            {
                Room = room;
                data.Users = room.GetUserInfos();
                Status = UserStatus.WAITTING;
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
            if (Status != UserStatus.WAITTING || Room == null)
            {
                data.Result = false;
                Emit(PacketType.LEAVE_ROOM_RES, data.ToBytes());
                return false;
            }
            Room = null;
            Status = UserStatus.LOBBY;
            Emit(PacketType.LEAVE_ROOM_RES, data.ToBytes());
            return true;
        }
        public void GameStart(bool isMafia, List<User> team)
        {
            if (Status != UserStatus.WAITTING)
                return;
            var data = new GameStartData(isMafia);
            if (isMafia)
                data.Mafias = (from u in team select u.Id).ToArray();
            Status = UserStatus.PLAYING;
            Emit(PacketType.GAME_START, data.ToBytes());
        }
        public UserInfo GetInfo()
        {
            return new UserInfo(Id, Name);
        }
        #endregion
    }
}
