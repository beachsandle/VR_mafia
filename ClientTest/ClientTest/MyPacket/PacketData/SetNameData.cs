using System;
using System.Collections.Generic;
using System.Text;

namespace MyPacket
{
    struct SetNameData : IPacketData
    {
        private int size;
        private string userName;
        public int Size
        {
            get
            {
                return size;
            }
        }
        public string UserName
        {
            get
            {
                return userName;
            }
            set
            {
                userName = value;
                size = ToBytes().Length;
            }
        }
        public SetNameData(string userName = "")
        {
            this.userName = userName;
            size = Encoding.UTF8.GetBytes(userName).Length;
        }
        public byte[] ToBytes()
        {
            return Encoding.UTF8.GetBytes(userName);
        }

        public void FromBytes(byte[] bytes)
        {
            size = bytes.Length;
            userName = Encoding.UTF8.GetString(bytes);
        }
    }
}
