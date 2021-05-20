using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Linq;

namespace MyPacket
{
    public class User : MySocket
    {
        #region field
        private static int playerId = 1;
        private readonly GameServer server;
        #endregion

        #region property
        public int Id { get; private set; }
        public string Name { get; private set; }
        public GameRoom Room { get; private set; }
        public bool IsMafia { get; private set; } = false;
        public bool Alive { get; private set; } = true;
        public GameStatus Status { get; private set; } = GameStatus.CONNECT;
        public Location Transform { get; private set; } = new Location();
        #endregion

        #region constructor
        public User(TcpClient client, GameServer server) : base(client)
        {
            Id = playerId++;
            Name = $"Player{Id:X}";
            this.server = server;
            EnrollHandler();
        }
        #endregion

        #region public method
        public bool LeaveRoom()
        {
            //대기실인 경우 퇴장 처리
            if (Status == GameStatus.WAITTING)
            {
                Room.Leave(this);
                Room = null;
                Status = GameStatus.LOBBY;
                return true;
            }
            return false;
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
        //유저 정보 반환
        public UserInfo GetInfo()
        {
            return new UserInfo(Id, Name);
        }
        //마피아 직업 부여
        public void SetMafia()
        {
            IsMafia = true;
        }
        //유저 이동
        public void MoveTo(Location transform)
        {
            Transform = transform;
        }
        #endregion

        #region eventhandler

        //유저의 이벤트 핸들러 등록
        private void EnrollHandler()
        {
            //connect
            On(PacketType.CONNECT, OnConnect);
            On(PacketType.DISCONNECT, OnDisconnect);
            //lobby
            On(PacketType.SET_NAME_REQ, OnSetNameReq);
            On(PacketType.ROOM_LIST_REQ, OnRoomListReq);
            On(PacketType.CREATE_ROOM_REQ, OnCreateRoomReq);
            On(PacketType.JOIN_ROOM_REQ, OnJoinRoomReq);
            //watting
            On(PacketType.LEAVE_ROOM_REQ, OnLeaveRoomReq);
            On(PacketType.GAME_START_REQ, OnGameStartReq);
            //ingame
            for (PacketType type = PacketType.INGAME_PACKET + 1; type < PacketType.END; ++type)
            {
                On(type, (Packet packet) => { Room.Enqueue(Id, packet); });
            }
        }
        private void OnConnect(Packet packet)
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
        private void OnDisconnect(Packet packet)
        {
            Close();
            switch (Status)
            {
                case GameStatus.WAITTING:
                    LeaveRoom();
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
        private void OnSetNameReq(Packet packet)
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
            var sendData = new LeaveResData(LeaveRoom());
            Emit(PacketType.LEAVE_ROOM_RES, sendData.ToBytes());
            if (sendData.Result)
                Console.WriteLine($"leave : {Id}");
        }
        private void OnGameStartReq(Packet packet)
        {
            Room.GameStart(this);
        }
        #endregion
    }
}
