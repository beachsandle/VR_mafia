using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyPacket
{
    class GameRoomInfo
    {
        public int Id { get; set; }
        public int HostId { get; set; }
        public int Participants { get; set; }
        public string Name { get; set; } = "";
        public int Size
        {
            get
            {
                return sizeof(int) * 4 + Encoding.UTF8.GetBytes(Name).Length; ;
            }
        }
        public GameRoomInfo() { }
        public GameRoomInfo(int id, int hostId,int num, string name)
        {
            Id = id;
            HostId = hostId;
            Participants = num;
            Name = name;
        }
        public byte[] ToBytes()
        {
            byte[] bytes = new byte[Size];
            Array.Copy(BitConverter.GetBytes(Size), 0, bytes, 0, 4);
            Array.Copy(BitConverter.GetBytes(Id), 0, bytes, 4, 4);
            Array.Copy(BitConverter.GetBytes(HostId), 0, bytes, 8, 4);
            Array.Copy(BitConverter.GetBytes(Participants), 0, bytes, 12, 4);
            var encoded = Encoding.UTF8.GetBytes(Name);
            Array.Copy(encoded, 0, bytes, 16, encoded.Length);
            return bytes;
        }
        public void FromBytes(byte[] bytes)
        {
            Id = BitConverter.ToInt32(bytes, 0);
            HostId = BitConverter.ToInt32(bytes, 4);
            Participants = BitConverter.ToInt32(bytes, 8);
            Name = Encoding.UTF8.GetString(bytes, 12, bytes.Length - 16);
        }
    }
}
