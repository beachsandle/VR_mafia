using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyPacket
{
    public class UserInfo
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
        public UserInfo(byte[] bytes = null)
        {
            if (bytes != null)
                FromBytes(bytes);
        }

        public UserInfo(int id, string name)
        {
            Id = id;
            Name = name;
        }
        public byte[] ToBytes()
        {
            return new ByteBuilder(Size)
                   .Append(Size)
                   .Append(Id)
                   .Append(Name)
                   .Get();
        }
        public void FromBytes(byte[] bytes)
        {
            int size = BitConverter.ToInt32(bytes, 0);
            Id = BitConverter.ToInt32(bytes, 4);
            Name = Encoding.UTF8.GetString(bytes, 8, size - 8);
        }
    }
}
