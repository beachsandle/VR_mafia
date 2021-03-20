using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyPacket
{
    class GameRoom
    {
        private static int roomId = 1;
        Dictionary<int, User> users;
        public int Id { get; private set; }
        public int HostId { get; private set; }
        public int Participants { get; private set; } = 0;
        public static int Maximum = 10;
        public string Name { get; set; }
        public bool IsStarted { get; private set; } = false;
        public GameRoom(User host = null, string name = "")
        {
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
        }
        public void Broadcast(PacketType type, byte[] bytes = null, User sender = null)
        {
            foreach (var p in users)
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
        public List<UserInfo> GetUserInfos()
        {
            return (from u in users.Values select u.GetInfo()).ToList();
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
    }
}
