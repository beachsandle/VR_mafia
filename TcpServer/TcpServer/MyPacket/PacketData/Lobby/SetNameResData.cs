using System;
using System.Collections.Generic;
using System.Text;

namespace MyPacket
{
    public class SetNameResData : IPacketData
    {
        public int Size
        {
            get
            {
                return 1 + Encoding.UTF8.GetBytes(UserName).Length;
            }
        }
        public bool Result = true;
        public string UserName = "";
        public SetNameResData(byte[] bytes = null)
        {
            if (bytes != null)
                FromBytes(bytes);
        }
        public SetNameResData(bool result, string userName = "")
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
