using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyPacket
{
    class UserInfo
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public int Size
        {
            get
            {
                return sizeof(int) * 2 + Encoding.UTF8.GetBytes(Name).Length; ;
            }
        }
        public UserInfo() { }
        public UserInfo(int id, string name)
        {
            Id = id;
            Name = name;
        }
        public byte[] ToBytes()
        {
            byte[] bytes = new byte[Size];
            Array.Copy(BitConverter.GetBytes(Size), 0, bytes, 0, 4);
            Array.Copy(BitConverter.GetBytes(Id), 0, bytes, 4, 4);
            var encoded = Encoding.UTF8.GetBytes(Name);
            Array.Copy(encoded, 0, bytes, 8, encoded.Length);
            return bytes;
        }
        public void FromBytes(byte[] bytes)
        {
            Id = BitConverter.ToInt32(bytes, 0);
            Name = Encoding.UTF8.GetString(bytes, 4, bytes.Length - 8);
        }
    }
}
