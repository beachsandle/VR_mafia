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
        private Dictionary<int, User> users;
        private const float DAY_TIME = 5;
        private const float NIGHT_TIME = 5;
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
            users = new Dictionary<int, User>();
            if (host != null)
            {
                HostId = host.Id;
                Name = name;
            }
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
            var thread = new Thread(GameLogic);
            thread.Start();
            Console.WriteLine($"start : {user.Id}");
            return true;
        }
        private void GameLogic()
        {
            var sw = new Stopwatch();
            float timer = DAY_TIME;
            long prev = 0, current;
            sw.Start();
            while (true)
            {
                Thread.Sleep(1);
                current = sw.ElapsedMilliseconds;
                timer -= (float)(current - prev) / 1000;
                prev = current;
                if (timer < 0)
                {
                    switch (Status)
                    {
                        case GameStatus.DAY:
                            timer = NIGHT_TIME;
                            Status = GameStatus.NIGHT;
                            Broadcast(PacketType.NIGHT_START);
                            break;
                        case GameStatus.NIGHT:
                            timer = DAY_TIME;
                            Status = GameStatus.DAY;
                            Broadcast(PacketType.DAY_START);
                            break;
                        case GameStatus.VOTE1: break;
                        case GameStatus.DEFENSE: break;
                        case GameStatus.VOTE2: break;
                        default: break;
                    }
                    prev = 0;
                    sw.Restart();
                }
            }
        }
    }
}
