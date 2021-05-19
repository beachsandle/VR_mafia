using System.Collections.Generic;
using System.Linq;
using System;
using System.Diagnostics;
using System.Threading;

namespace MyPacket
{
    public class GameRoom
    {
        #region field
        private static int roomId = 1;
        private readonly Dictionary<int, User> users = new Dictionary<int, User>();
        private readonly Queue<(int id, Packet packet)> eventQueue = new Queue<(int id, Packet packet)>();
        private delegate void EventHandler((int, Packet) eventData);
        private readonly Dictionary<PacketType, EventHandler> handlerMap = new Dictionary<PacketType, EventHandler>();
        private readonly Stopwatch timer = new Stopwatch();
        private float currentTime = 0;
        private long prevTime = 0;
        private const float DAY_TIME = 5;
        private const float NIGHT_TIME = 5;
        private const float VOTE_TIME = 10;
        private const float VOTE_RESULT_TIME = 3;
        private const float DEFENSE_TIME = 5;
        private const float FINAL_VOTE_TIME = 10;
        #endregion
        #region property
        public int Id { get; private set; }
        public int HostId { get; private set; }
        public int Participants { get; private set; } = 0;
        public static int Maximum = 10;
        public string Name { get; set; }
        public GameStatus Status { get; private set; } = GameStatus.WAITTING;
        public GameServer Server { get; private set; }
        #endregion
        public GameRoom(GameServer server, User host = null, string name = "")
        {
            Server = server;
            Id = roomId++;
            if (host != null)
            {
                HostId = host.Id;
                Name = name;
            }
            InitHandlerMap();
        }
        //모든 패킷 종류에 대한 핸들러를 공백 핸들러로 초기화
        private void InitHandlerMap()
        {
            foreach (PacketType type in Enum.GetValues(typeof(PacketType)))
            {
                handlerMap[type] = EmptyHandler;
            }
        }
        //공백 핸들러
        private void EmptyHandler((int, Packet) data) { }
        //이벤트 핸들러 등록
        private void On(PacketType type, EventHandler handler)
        {
            handlerMap[type] += handler;
        }
        public void Enqueue(int id, Packet packet)
        {
            eventQueue.Enqueue((id, packet));
        }
        //TODO: 게임룸 내에 큐를 만들고 동기적으로 하고싶음
        public bool Join(User user)
        {
            if (Participants == Maximum)
                return false;
            users[user.Id] = user;
            ++Participants;
            var data = new JoinEventData(user.GetInfo());
            Broadcast(PacketType.JOIN_EVENT, data.ToBytes(), user);
            return true;
        }
        public void RemoveUser(int userId)
        {
            --Participants;
            users.Remove(userId);
        }
        public bool Leave(User user)
        {
            if (!user.LeaveRoom())
                return false;
            RemoveUser(user.Id);
            if (Status == GameStatus.WAITTING && HostId == user.Id)
            {
                foreach (var u in users.Values.ToArray())
                {
                    if (u.LeaveRoom())
                    {
                        RemoveUser(u.Id);
                    }
                }
            }
            Broadcast(PacketType.LEAVE_EVENT, new LeaveEventData(user.Id).ToBytes());
            if (Participants == 0)
                Server.RemoveRoom(Id);
            return true;
        }
        public void Broadcast(PacketType type, byte[] bytes = null, User sender = null)
        {
            foreach (var p in users.ToArray())
            {
                if (sender?.Id == p.Value.Id)
                    continue;
                p.Value.Emit(type, bytes);
            }
        }
        public GameRoomInfo GetInfo()
        {
            return new GameRoomInfo(Id, HostId, Participants, Name);
        }
        public void On(PacketType type, MySocket.MessageHandler handler)
        {
            foreach (var user in users.Values)
            {
                user.On(type, handler);
            }
        }
        public void Clear(PacketType type)
        {
            foreach (var user in users.Values)
            {
                user.Clear(type);
            }
        }
        public List<UserInfo> GetUserInfos()
        {
            return (from u in users.Values select u.GetInfo()).ToList();
        }
        private List<User> DesideMafia()
        {
            var mafias = users.Values
                        .OrderBy(a => Guid.NewGuid())
                        .Take(Participants > 6 ? 2 : 1);
            foreach (var u in mafias)
                u.SetMafia();
            return mafias.ToList();
        }
        public bool GameStart(User user)
        {
            //대기실이고, 방장일 경우에만 게임 시작 가능
            if (user.Status != GameStatus.WAITTING || HostId != user.Id)
                return false;
            Status = GameStatus.DAY;
            var mafias = DesideMafia();
            foreach (var u in users.Values)
            {
                u.GameStart(u.IsMafia, mafias);
            }
            EnrollHanders();
            var thread = new Thread(GameLoof);
            thread.Start();
            Console.WriteLine($"start : {user.Id}");
            return true;
        }
        private void EnrollHanders()
        {
            On(PacketType.MOVE_REQ, OnMoveReq);
        }
        private void TimeFlow()
        {
            long mill = timer.ElapsedMilliseconds;
            currentTime -= (float)(mill - prevTime) / 1000;
            prevTime = mill;
        }
        private void EventHandling()
        {
            while (eventQueue.Count > 0)
            {
                var data = eventQueue.Dequeue();
                handlerMap[data.packet.Header.Type](data);
            }
        }
        private void StartNight()
        {
            currentTime = NIGHT_TIME;
            Status = GameStatus.NIGHT;
            Broadcast(PacketType.NIGHT_START);
        }
        private void StartVoting()
        {
            currentTime = VOTE_TIME;
            Status = GameStatus.VOTE;
            Broadcast(PacketType.START_VOTING, new StartVotingData((int)VOTE_TIME).ToBytes());
        }
        private void EndVoting()
        {
            currentTime = VOTE_RESULT_TIME;
            Status = GameStatus.VOTE_RESULT;
            Broadcast(PacketType.VOTING_RESULT);

        }
        private void StartDefense()
        {
            currentTime = DEFENSE_TIME;
            Status = GameStatus.DEFENSE;
            Broadcast(PacketType.START_DEFENCE, new StartDefenseData((int)DEFENSE_TIME, 0).ToBytes());
        }
        private void StartFinalVotinig()
        {
            currentTime = FINAL_VOTE_TIME;
            Status = GameStatus.FINAL_VOTE;
            Broadcast(PacketType.START_FINAL_VOTING, new StartFinalVotingData((int)FINAL_VOTE_TIME).ToBytes());
        }
        private void EndFinalVotinig()
        {
            StartDay();
        }
        private void StartDay()
        {
            currentTime = DAY_TIME;
            Status = GameStatus.DAY;
            Broadcast(PacketType.DAY_START);

        }
        //시간이 만료되면 발생하는 이벤트
        private void Timeout()
        {
            switch (Status)
            {
                case GameStatus.DAY: StartNight(); break;
                case GameStatus.NIGHT: StartVoting(); break;
                case GameStatus.VOTE: EndVoting(); break;
                case GameStatus.VOTE_RESULT: StartDefense(); break;
                case GameStatus.DEFENSE: StartFinalVotinig(); break;
                case GameStatus.FINAL_VOTE: EndFinalVotinig(); break;
                default: break;
            }
            prevTime = 0;
            timer.Restart();
        }
        //게임로직을 담당하는 루프
        private void GameLoof()
        {
            currentTime = DAY_TIME;
            timer.Start();
            while (Status != GameStatus.END)
            {
                Thread.Sleep(1);
                EventHandling();
                TimeFlow();
                if (currentTime < 0)
                    Timeout();
            }
            timer.Stop();
        }
        private void OnMoveReq((int, Packet) eventdata)
        {
            (var id, var packet) = eventdata;
            if (!users.ContainsKey(id))
                return;
            var data = new MoveReqData(packet.Bytes);
            users[id].Transform = data.location;
            var sendData = new MoveEventData(id, data.location);
            Broadcast(PacketType.MOVE_EVENT, sendData.ToBytes(), users[id]);
        }
    }
}
