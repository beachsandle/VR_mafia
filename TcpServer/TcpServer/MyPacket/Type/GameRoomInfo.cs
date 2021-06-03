using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyPacket
{
    public class GameRoomInfo
    {
        public int Id { get; set; }
        public int Participants { get; set; }
        public UserInfo Host { get; set; }
        public string Name { get; set; } = "";
        public int Size
        {
            get
            {
                return sizeof(int) * 3 + Host.Size + Encoding.UTF8.GetBytes(Name).Length; ;
            }
        }
        public GameRoomInfo(byte[] bytes = null)
        {
            if (bytes != null)
                FromBytes(bytes);
        }
        public GameRoomInfo(int id, int num, UserInfo host, string name)
        {
            Id = id;
            Host = host;
            Participants = num;
            Name = name;
        }
        public byte[] ToBytes()
        {
            return new ByteBuilder(Size)
                .Append(Size)
                .Append(Id)
                .Append(Participants)
                .Append(Host.ToBytes())
                .Append(Name)
                .Get();
        }
        public void FromBytes(byte[] bytes)
        {
            int size = BitConverter.ToInt32(bytes, 0);
            Id = BitConverter.ToInt32(bytes, 4);
            Participants = BitConverter.ToInt32(bytes, 8);
            Host = new UserInfo(bytes.Skip(12).ToArray());
            Name = Encoding.UTF8.GetString(bytes, 12 + Host.Size, size - 12 - Host.Size);
        }
    }
}
