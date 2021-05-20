using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Linq;

namespace MyPacket
{
    public class User : MySocket
    {
        private static int playerId = 1;
        private readonly GameServer server;
        #region property
        public Location Transform { get; set; } = new Location();
        public bool Moved { get; set; } = false;
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
            Name = $"Player{Id:X}";
            this.server = server;
        }
        #endregion
        #region public method
        #region connect
        public void EnrollHandler()
        {
            On(PacketType.CONNECT, OnConnect);
            On(PacketType.DISCONNECT, OnDisconnect);

            On(PacketType.SET_NAME_REQ, OnSetNameReq);
            On(PacketType.ROOM_LIST_REQ, OnRoomListReq);
            On(PacketType.CREATE_ROOM_REQ, OnCreateRoomReq);
            On(PacketType.JOIN_ROOM_REQ, OnJoinRoomReq);

            On(PacketType.LEAVE_ROOM_REQ, OnLeaveRoomReq);
            On(PacketType.GAME_START_REQ, OnGameStartReq);

            for (PacketType type = PacketType.INGAME_PACKET + 1; type < PacketType.END; ++type)
            {
                On(type, (Packet packet) => { Room.Enqueue(Id, packet); });
            }

        }
        //status가 connect일 경우 로비로 이동하고 connect, set name res 전송
        public void OnConnect(Packet packet)
        {
            if (Status == GameStatus.CONNECT)
            {
                Status = GameStatus.LOBBY;
                Emit(PacketType.CONNECT, new ConnectData(Id).ToBytes());
                Emit(PacketType.SET_NAME_RES, new SetNameResData(true, Name).ToBytes());
            }
            Console.WriteLine($"connect : {Id}");
        }
        //
        public void OnDisconnect(Packet packet)
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
                case GameStatus.VOTE:
                case GameStatus.DEFENSE:
                case GameStatus.FINAL_VOTE:
                    Room.RemoveUser(Id);
                    break;
            }
            server.RemoveUser(Id);
            Console.WriteLine($"disconnect : {Id}, {Name}");
        }
        #endregion
        public void OnSetNameReq(Packet packet)
        {
            var data = new SetNameReqData(packet.Bytes);
            var sendData = new SetNameResData();
            //로비가 아닐 경우 실패
            if (Status != GameStatus.LOBBY)
            {
                sendData.Result = false;
                Emit(PacketType.SET_NAME_RES, sendData.ToBytes());
                return;
            }
            //이름을 변경하고 결과 전송
            Name = data.UserName;
            sendData.UserName = data.UserName;
            Emit(PacketType.SET_NAME_RES, sendData.ToBytes());
            Console.WriteLine($"setname : {Id}, {Name}");
        }
        private void OnRoomListReq(Packet packet)
        {
            var sendData = new RoomListResData();
            if (Status != GameStatus.LOBBY)
            {
                sendData.Result = false;
                Emit(PacketType.ROOM_LIST_RES, sendData.ToBytes());
                return;
            }
            sendData.Rooms = server.GetRoomInfos();
            Emit(PacketType.ROOM_LIST_RES, sendData.ToBytes());
            Console.WriteLine($"room list req : {Id}");
        }
        private void OnCreateRoomReq(Packet packet)
        {
            var data = new CreateRoomReqData(packet.Bytes);
            var sendData = new CreateRoomResData();
            if (Status != GameStatus.LOBBY)
            {
                sendData.Result = false;
                Emit(PacketType.CREATE_ROOM_RES, sendData.ToBytes());
                return;
            }
            Room = server.CreateRoom(this, data.RoomName);
            Room.Join(this);
            Status = GameStatus.WAITTING;
            Emit(PacketType.CREATE_ROOM_RES, sendData.ToBytes());
            Console.WriteLine($"create room req : {Id}");
        }
        private void OnJoinRoomReq(Packet packet)
        {
            var data = new JoinRoomReqData(packet.Bytes);
            var sendData = new JoinRoomResData();
            var room = server.FindRoomById(data.RoomId);
            if (Status != GameStatus.LOBBY || room == null)
            {
                sendData.Result = false;
                Emit(PacketType.JOIN_ROOM_RES, sendData.ToBytes());
                return;
            }
            if (room.Join(this))
            {
                Room = room;
                sendData.Users = room.GetUserInfos();
                Status = GameStatus.WAITTING;
                Emit(PacketType.JOIN_ROOM_RES, sendData.ToBytes());
                return;
            }
            sendData.Result = false;
            Emit(PacketType.JOIN_ROOM_RES, sendData.ToBytes());
            Console.WriteLine($"join room : {Id}");
        }
        private void OnLeaveRoomReq(Packet packet)
        {
            if (LeaveRoom())
                Console.WriteLine($"leave : {Id}");
        }
        public bool LeaveRoom()
        {
            var sendData = new LeaveResData();
            if (Status != GameStatus.WAITTING || Room == null)
            {
                sendData.Result = false;
                Emit(PacketType.LEAVE_ROOM_RES, sendData.ToBytes());
                return false;
            }
            Room = null;
            Status = GameStatus.LOBBY;
            Emit(PacketType.LEAVE_ROOM_RES, sendData.ToBytes());
            return true;
        }
        private void OnGameStartReq(Packet packet)
        {
            Room.GameStart(this);
        }
        public void GameStart(bool isMafia, List<User> team)
        {
            if (Status != GameStatus.WAITTING)
                return;
            var sendData = new GameStartData(isMafia);
            if (isMafia)
                sendData.Mafias = (from u in team select u.Id).ToArray();
            Status = GameStatus.DAY;
            Emit(PacketType.GAME_START, sendData.ToBytes());
        }
        public UserInfo GetInfo()
        {
            return new UserInfo(Id, Name);
        }
        public void SetMafia()
        {
            IsMafia = true;
        }
        #endregion
    }
}
