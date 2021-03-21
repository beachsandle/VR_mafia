using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyPacket
{
    public class GameRoom
    {
        #region field
        private static int roomId = 1;
        private Dictionary<int, User> users;
        #endregion
        #region property
        public int Id { get; private set; }
        public int HostId { get; private set; }
        public int Participants { get; private set; } = 0;
        public static int Maximum = 10;
        public string Name { get; set; }
        public bool IsStarted { get; private set; } = false;
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
        public bool Join(User user)
        {
            if (Participants == Maximum)
                return false;
            users[user.Id] = user;
            ++Participants;
            return true;
        }
        public void Leave(User user)
        {
            --Participants;
            users.Remove(user.Id);
            if (!IsStarted && HostId == user.Id)
            {
                foreach (var u in users.Values.ToArray())
                {
                    u.Emit(PacketType.LEAVE_EVENT, new LeaveEventData(u.Id).ToBytes());
                    u.LeaveRoom();
                    --Participants;
                    users.Remove(u.Id);
                }
            }
            if (Participants == 0)
                Server.Rooms.Remove(Id);
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
            return new GameRoomInfo(Id, HostId, Participants, IsStarted, Name);
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
        public void GameStart()
        {
            IsStarted = true;
            Broadcast(PacketType.GAME_START);
        }
    }
}
