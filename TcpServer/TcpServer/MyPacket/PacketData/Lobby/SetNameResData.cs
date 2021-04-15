using System;
using System.Collections.Generic;
using System.Text;

namespace MyPacket
{
    public struct SetNameResData : IPacketData
    {
        public int Size
        {
            get
            {
                return 1 + Encoding.UTF8.GetBytes(UserName).Length;
            }
        }
        public bool Result { get; set; }
        public string UserName { get; set; }
        public SetNameResData(bool result = true, string userName = "")
        {
            Result = result;
            UserName = userName;
        }
        public byte[] ToBytes()
        {
            return new ByteBuilder(Size).Append(Result).Append(UserName).Get();
        }

        public void FromBytes(byte[] bytes)
        {
            Result = BitConverter.ToBoolean(bytes, 0);
            UserName = Encoding.UTF8.GetString(bytes, 1, bytes.Length - 1);
        }
    }
}
