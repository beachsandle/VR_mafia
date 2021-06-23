using System.Collections.Generic;
using System.Linq;
using System;
using System.Diagnostics;
using System.Threading;

namespace MyPacket
{
    public class GameRoom
    {
        #region delegate
        private delegate void EventHandler((int, Packet) eventData);
        #endregion

        #region field
        private static int roomId = 1;
        private float currentTime = 0;
        private long prevTime = 0;
        private int voters = 0;
        private int maxVoters = 0;
        private int electedId = -1;
        private int live = 0;
        private readonly GameServer server;
        private readonly Dictionary<int, User> users = new Dictionary<int, User>();
        private readonly Queue<(int id, Packet packet)> eventQueue = new Queue<(int id, Packet packet)>();
        private readonly Dictionary<PacketType, EventHandler> handlerMap = new Dictionary<PacketType, EventHandler>();
        private readonly Stopwatch timer = new Stopwatch();
        #endregion

        #region constant
        private const float DAY_TIME = 5;
        private const float NIGHT_TIME = 5;
        private const float VOTE_TIME = 10;
        private const float VOTE_END_TIME = 3;
        private const float DEFENSE_TIME = 5;
        private const float FINAL_VOTE_TIME = 10;
        private const float FINAL_VOTE_END_TIME = 3;
        #endregion

        #region property
        public static int Maximum { get; private set; } = 10;
        public int RoomId { get; private set; }
        public int HostId { get; private set; }
        public int Participants
        {
            get
            {
                return users.Count;
            }
        }
        public string Name { get; private set; }
        public GameStatus Status { get; private set; } = GameStatus.WAITTING;
        #endregion

        #region constructor
        //게임룸 생성, id는  auto increase
        public GameRoom(GameServer server, User host = null, string name = "")
        {
            this.server = server;
            RoomId = roomId++;
            if (host != null)
            {
                HostId = host.Id;
                Name = name;
            }
            InitHandlerMap();
        }
        #endregion

        #region private method
        private List<User> DesideMafia()
        {
            //var mafias = users.Values
            //            .OrderBy(a => Guid.NewGuid())
            //            .Take(Participants > 6 ? 2 : 1);
            var mafias = new List<User>();
            mafias.Add(users[HostId]);
            foreach (var u in mafias)
                u.SetMafia();
            return mafias.ToList();
        }
        private void InitVotingPhase()
        {
            maxVoters = 0;
            foreach (var u in users.Values)
            {
                if (u.Alive)
                {
                    ++maxVoters;
                    u.ResetVoteStatus();
                }
            }
            voters = maxVoters;
        }
        #endregion

        #region life cycle
        private void EventHandling()
        {
            while (eventQueue.Count > 0)
            {
                (int, Packet) data;
                lock (eventQueue)
                    data = eventQueue.Dequeue();
                handlerMap[data.Item2.Header.Type](data);

            }
        }
        private void TimeFlow()
        {
            long mill = timer.ElapsedMilliseconds;
            currentTime -= (float)(mill - prevTime) / 1000;
            prevTime = mill;
        }
        private void StartNight()
        {
            if (Status != GameStatus.DAY) return;
            currentTime = NIGHT_TIME;
            Status = GameStatus.NIGHT;
            Broadcast(PacketType.NIGHT_START);

            server.Log($"night start : {RoomId}");
        }
        private void StartVoting()
        {
            if (Status != GameStatus.NIGHT) return;
            currentTime = VOTE_TIME;
            Status = GameStatus.VOTE;
            electedId = -1;
            InitVotingPhase();
            Broadcast(PacketType.START_VOTING, new StartVotingData((int)VOTE_TIME).ToBytes());

            server.Log($"voting start : {RoomId}");
        }
        private void EndVoting()
        {
            if (Status != GameStatus.VOTE) return;
            currentTime = VOTE_END_TIME;
            Status = GameStatus.VOTE_END;
            var result = from u in users.Values
                         orderby u.VoteCount descending
                         select (u.Id, u.VoteCount);
            var elected = result.First();
            //최대 득표수가 1이거나 동수일 경우
            if (elected.VoteCount < 2 ||
                elected.VoteCount == result.ElementAt(1).VoteCount)
                elected.Id = -1;
            electedId = elected.Id;
            var sendData = new VotingResultData(electedId, result.ToArray());
            Broadcast(PacketType.VOTING_RESULT, sendData.ToBytes());

            server.Log($"voting result : elected - {elected}");
        }
        private void HandlingVoteResult()
        {
            if (Status != GameStatus.VOTE_END) return;
            if (electedId == -1)
                StartDay();
            else
                StartDefense();
        }
        private void StartDefense()
        {
            if (Status != GameStatus.VOTE_END) return;
            currentTime = DEFENSE_TIME;
            Status = GameStatus.DEFENSE;
            Broadcast(PacketType.START_DEFENSE, new StartDefenseData((int)DEFENSE_TIME, electedId).ToBytes());
            server.Log($"defense start : {RoomId}, elected - {electedId}");
        }
        private void StartFinalVotinig()
        {
            if (Status != GameStatus.DEFENSE) return;
            currentTime = FINAL_VOTE_TIME;
            Status = GameStatus.FINAL_VOTE;
            InitVotingPhase();
            Broadcast(PacketType.START_FINAL_VOTING, new StartFinalVotingData((int)FINAL_VOTE_TIME).ToBytes());
            server.Log($"final voting start : {RoomId}");
        }
        private void EndFinalVoting()
        {
            if (Status != GameStatus.FINAL_VOTE) return;
            var sendData = new FinalVotingResultData(electedId, users[electedId].VoteCount);
            currentTime = FINAL_VOTE_END_TIME;
            Status = GameStatus.FINAL_VOTE_END;
            if (sendData.voteCount * 2 >= maxVoters)
            {
                users[electedId].Execute();
                --live;
            }
            else
                sendData.Kicking_id = -1;
            Broadcast(PacketType.FINAL_VOTING_RESULT, sendData.ToBytes());

            server.Log($"final voting result : elected - {electedId}, count - {sendData.voteCount}");
        }
        private void StartDay()
        {
            if (Status != GameStatus.FINAL_VOTE_END && Status != GameStatus.VOTE_END) return;
            currentTime = DAY_TIME;
            Status = GameStatus.DAY;
            Broadcast(PacketType.DAY_START);

            server.Log($"day start : {RoomId}");

        }
        //시간이 만료되면 발생하는 이벤트
        private void Timeout()
        {
            switch (Status)
            {
                case GameStatus.DAY: StartNight(); break;
                case GameStatus.NIGHT: StartVoting(); break;
                case GameStatus.VOTE: EndVoting(); break;
                case GameStatus.VOTE_END: HandlingVoteResult(); break;
                case GameStatus.DEFENSE: StartFinalVotinig(); break;
                case GameStatus.FINAL_VOTE: EndFinalVoting(); break;
                case GameStatus.FINAL_VOTE_END: StartDay(); break;
                default: break;
            }
            prevTime = 0;
            timer.Restart();
        }
        //게임로직을 담당하는 루프
        private void GameLoof()
        {
            EnrollHanders();
            currentTime = DAY_TIME;
            timer.Start();
            while (Status != GameStatus.END && live > 0)
            {
                if (eventQueue.Count == 0)
                    Thread.Sleep(1);
                EventHandling();
                //TimeFlow();
                if (currentTime < 0)
                    Timeout();
            }
            timer.Stop();
        }
        #endregion

        #region public method
        //이벤트 큐에 패킷 추가
        public void Enqueue(int id, Packet packet)
        {
            lock (eventQueue)
                eventQueue.Enqueue((id, packet));
        }
        //유저 참가 처리
        public bool Join(User user)
        {
            if (Participants == Maximum)
                return false;
            users[user.Id] = user;
            var data = new JoinEventData(user.GetInfo());
            Broadcast(PacketType.JOIN_EVENT, data.ToBytes(), user);
            return true;
        }
        //유저 제거
        public void RemoveUser(int userId)
        {
            if (users[userId].Alive)
                --live;
            users.Remove(userId);
        }
        //유저 퇴장 처리
        public bool Leave(User user)
        {
            if (Status != GameStatus.WAITTING)
                return false;
            user.LeaveRoom();
            RemoveUser(user.Id);
            // 방장이 퇴장한 경우
            if (HostId == user.Id)
            {
                //모든 유저에게 ROOM_DESTROY_EVENT 발생 후 방 제거
                Broadcast(PacketType.ROOM_DESTROY_EVENT);
                //모든 유저 퇴장
                foreach (var u in users.Values.ToArray())
                {
                    u.LeaveRoom();
                    RemoveUser(u.Id);
                }

                server.Log($"room destroy : {RoomId}");
                server.RemoveRoom(RoomId);
            }
            //그 외엔 남은 유저들에게 퇴장사실 전달
            else
                Broadcast(PacketType.LEAVE_EVENT, new LeaveEventData(user.Id).ToBytes());
            return true;
        }
        //게임방의 정보를 반환
        public GameRoomInfo GetInfo()
        {
            return new GameRoomInfo(RoomId, Participants, users[HostId].GetInfo(), Name);
        }

        //유저들의 정보를 반환
        public List<UserInfo> GetUserInfos()
        {
            return (from u in users.Values select u.GetInfo()).ToList();
        }
        //게임 시작
        public bool GameStart(User user)
        {
            //대기실이고, 방장일 경우에만 게임 시작 가능
            if (user.Status != GameStatus.WAITTING || HostId != user.Id)
                return false;
            //if (users.Count < 4) return false;
            Status = GameStatus.DAY;
            live = users.Count;
            var mafias = DesideMafia();
            foreach (var u in users.Values)
            {
                u.GameStart(u.IsMafia, mafias);
            }
            var thread = new Thread(GameLoof);
            thread.Start();

            server.Log($"start : {user.Id}");
            return true;
        }
        #endregion

        #region eventhandler
        //이벤트 핸들러 등록
        private void On(PacketType type, EventHandler handler)
        {
            handlerMap[type] += handler;
        }
        //모든 유저에게 송신
        private void Broadcast(PacketType type, byte[] bytes = null, User sender = null)
        {
            foreach (var p in users.ToArray())
            {
                if (sender?.Id == p.Value.Id)
                    continue;
                p.Value.Emit(type, bytes);
            }
        }
        //모든 패킷 종류에 대한 핸들러를 공백 핸들러로 초기화
        private void InitHandlerMap()
        {
            foreach (PacketType type in Enum.GetValues(typeof(PacketType)))
            {
                handlerMap[type] = EmptyHandler;
            }
        }
        //핸들러 등록
        private void EnrollHanders()
        {
            On(PacketType.MOVE_REQ, OnMoveReq);
            On(PacketType.VOTE_REQ, OnVoteReq);
            On(PacketType.FINAL_VOTE_REQ, OnFinalVoteReq);
            On(PacketType.DEAD_REPORT, OnDeadReport);
            On(PacketType.KILL_REQ, OnKillReq);
        }
        //공백 핸들러
        private void EmptyHandler((int, Packet) data) { }
        //move 이벤트 핸들러
        private void OnMoveReq((int, Packet) eventdata)
        {
            (var id, var packet) = eventdata;
            if (!users.ContainsKey(id))
                return;
            var data = new MoveReqData(packet.Bytes);
            var sendData = new MoveEventData(id, data.location);
            users[id].MoveTo(data.location);
            Broadcast(PacketType.MOVE_EVENT, sendData.ToBytes(), users[id]);
        }
        private void OnVoteReq((int, Packet) eventdata)
        {
            (int id, Packet packet) = eventdata;
            var data = new VoteReqData(packet.Bytes);
            var sendData = new VoteResData();
            if (Status == GameStatus.VOTE)
            {
                if (users.ContainsKey(data.Target_id) &&
                    users[data.Target_id].Alive &&
                    users[id].Alive &&
                    !users[id].Voted)
                {
                    users[id].Vote();
                    users[data.Target_id].IsVoted();
                    --voters;
                }
                else
                    sendData.Result = false;
            }
            else
                sendData.Result = false;
            server.Log($"vote : {id} -> {data.Target_id} {sendData.Result}");
            users[id].Emit(PacketType.VOTE_RES, sendData.ToBytes());
            if (voters == 0)
                EndVoting();
        }
        private void OnFinalVoteReq((int, Packet) eventdata)
        {
            (int id, Packet packet) = eventdata;
            var data = new FinalVoteReqData(packet.Bytes);
            var sendData = new FinalVoteResData();
            server.Log($"final vote : {id} -> {data.Agree}");
            if (Status == GameStatus.FINAL_VOTE)
            {
                if (users[id].Alive &&
                    !users[id].Voted)
                {
                    users[id].Vote();
                    --voters;
                    if (data.Agree)
                        users[electedId].IsVoted();
                }
                else
                    sendData.Result = false;
            }
            else
                sendData.Result = false;
            users[id].Emit(PacketType.FINAL_VOTE_RES, sendData.ToBytes());
            if (voters == 0)
                EndFinalVoting();
        }
        private void OnDeadReport((int, Packet) eventdata)
        {
            (_, Packet packet) = eventdata;
            var data = new DeadReportData(packet.Bytes);

            if (Status == GameStatus.DAY)
            {
                if (users[data.Reporter_id].Alive && users[data.Dead_id].IsDeadBody)
                {
                    users[data.Dead_id].Reported();
                    Broadcast(PacketType.DEAD_REPORT, data.ToBytes());
                    StartNight();
                    server.Log($"dead report : {data.Reporter_id} -> {data.Dead_id}");
                }
            }
        }
        private void OnKillReq((int, Packet) eventdata)
        {
            (int id, Packet packet) = eventdata;
            var data = new KillReqDada(packet.Bytes);
            var sendData = new KillResDada(false);

            if (Status == GameStatus.DAY &&
                users.ContainsKey(data.Target_id) &&
                users[id].Kill(users[data.Target_id]))
            {
                --live;
                sendData.Result = true;
            }

            users[id].Emit(PacketType.KILL_RES, sendData.ToBytes());
            if (sendData.Result)
                Broadcast(PacketType.DIE_EVENT, new DieEventData(data.Target_id).ToBytes());
        }
        #endregion
    }
}
