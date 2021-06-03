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
        public bool IsDeadBody { get; private set; } = false;
        public bool HasBullet { get; private set; } = false;
        public bool Voted { get; private set; } = false;
        public int VoteCount { get; private set; } = 0;
        public GameStatus Status { get; private set; } = GameStatus.CONNECT;
        public Location Transform { get; private set; } = new Location();

        public bool KillReady
        {
            get
            {
                return Alive && IsMafia && HasBullet;
            }
        }
        #endregion

        #region constructor
        public User(Socket socket, GameServer server) : base(socket)
        {
            Id = playerId++;
            Name = $"Player{Id:X}";
            this.server = server;
            EnrollHandler();
        }
        #endregion

        #region public method
        //유저를 퇴장시킴
        public void LeaveRoom()
        {
            Room = null;
            Status = GameStatus.LOBBY;
        }
        public void GameStart(bool isMafia, List<User> team)
        {
            if (Status != GameStatus.WAITTING)
                return;
            var sendData = new GameStartData(isMafia);
            if (isMafia)
                sendData.Mafias = (from u in team select u.Id).ToArray();
            Status = GameStatus.INGAME;
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
            HasBullet = true;
        }
        public void ResetVoteStatus()
        {
            Voted = false;
            VoteCount = 0;
        }
        public void Vote()
        {
            Voted = true;
        }
        public void IsVoted()
        {
            ++VoteCount;
        }
        //유저 이동
        public void MoveTo(Location transform)
        {
            Transform = transform;
        }
        public void Execute()
        {
            ResetVoteStatus();
            Alive = false;
        }
        public void Killed()
        {
            Alive = false;
            IsDeadBody = true;
        }
        public bool Kill(User target)
        {
            if (Alive && IsMafia && HasBullet)
            {
                if (target.Alive && !target.IsMafia)
                {
                    HasBullet = false;
                    target.Killed();
                    return true;
                }
            }
            return false;
        }
        public void Reported()
        {
            IsDeadBody = false;
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
            }
            server.Log($"connect : {Id}");
        }
        private void OnDisconnect(Packet packet)
        {
            if (Status == GameStatus.NONE) return;
            switch (Status)
            {
                case GameStatus.WAITTING:
                    Room.Leave(this);
                    break;
                case GameStatus.INGAME:
                    Room.RemoveUser(Id);
                    break;
            }
            Status = GameStatus.NONE;
            server.RemoveUser(Id);
            server.Log($"disconnect : {Id}, {Name}");
        }
        private void OnSetNameReq(Packet packet)
        {
            var data = new SetNameReqData(packet.Bytes);
            var sendData = new SetNameResData();
            //로비일 경우 이름 변경
            if (Status == GameStatus.LOBBY)
            {
                Name = data.UserName;
                sendData.UserName = data.UserName;
            }
            else
                sendData.Result = false;
            //결과 전송
            Emit(PacketType.SET_NAME_RES, sendData.ToBytes());
            if (sendData.Result)
                server.Log($"setname : {Id}, {Name}");
        }
        private void OnRoomListReq(Packet packet)
        {
            var sendData = new RoomListResData();
            //로비일경우 방목록 저장 
            if (Status == GameStatus.LOBBY)
                sendData.Rooms = server.GetRoomInfos();
            else
                sendData.Result = false;
            //결과 전송
            Emit(PacketType.ROOM_LIST_RES, sendData.ToBytes());
            if (sendData.Result)
                server.Log($"room list req : {Id}");
        }
        private void OnCreateRoomReq(Packet packet)
        {
            var data = new CreateRoomReqData(packet.Bytes);
            var sendData = new CreateRoomResData();
            //로비일 경우 방을 생성하고 대기실로 입장
            if (Status == GameStatus.LOBBY)
            {
                Room = server.CreateRoom(this, data.RoomName);
                Room.Join(this);
                Status = GameStatus.WAITTING;
            }
            else
                sendData.Result = false;
            //결과 전송
            Emit(PacketType.CREATE_ROOM_RES, sendData.ToBytes());
            if (sendData.Result)
                server.Log($"create room req : {Id}, {data.RoomName}");
        }
        private void OnJoinRoomReq(Packet packet)
        {
            var data = new JoinRoomReqData(packet.Bytes);
            var sendData = new JoinRoomResData();
            var room = server.FindRoomById(data.RoomId);
            //로비이고 방 참여에 성공했을 경우
            if (Status == GameStatus.LOBBY && (room?.Join(this) ?? false))
            {
                Room = room;
                sendData.Users = room.GetUserInfos();
                Status = GameStatus.WAITTING;
            }
            else
                sendData.Result = false;
            //결과 전송
            Emit(PacketType.JOIN_ROOM_RES, sendData.ToBytes());
            if (sendData.Result)
                server.Log($"join room : {Id}");
        }
        private void OnLeaveRoomReq(Packet packet)
        {
            var sendData = new LeaveResData(
                Status == GameStatus.WAITTING &&
                Room.Leave(this));
            Emit(PacketType.LEAVE_ROOM_RES, sendData.ToBytes());
            if (sendData.Result)
                server.Log($"leave : {Id}");
        }
        private void OnGameStartReq(Packet packet)
        {
            if (Status == GameStatus.WAITTING)
                Room.GameStart(this);
        }
        #endregion
    }
}
