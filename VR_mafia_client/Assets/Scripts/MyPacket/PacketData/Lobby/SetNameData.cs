using System;
using System.Collections.Generic;
using System.Text;

namespace MyPacket
{
    public struct SetNameData : IPacketData
    {
        public int Size
        {
            get
            {
                return Encoding.UTF8.GetBytes(UserName).Length;
            }
        }
        public string UserName { get; set; }
        public SetNameData(string userName = "")
        {
            UserName = userName;
        }
        public byte[] ToBytes()
        {
            return Encoding.UTF8.GetBytes(UserName);
        }

        public void FromBytes(byte[] bytes)
        {
            UserName = Encoding.UTF8.GetString(bytes);
        }
    }
}
