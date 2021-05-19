using System;
using System.Collections.Generic;
using System.Text;

namespace MyPacket
{
    public class CreateRoomResData : IPacketData
    {
        public bool Result = true;
        public int Size
        {
            get
            {
                return 1;
            }
        }
        public CreateRoomResData(byte[] bytes = null)
        {
            if (bytes != null)
                FromBytes(bytes);
        }
        public CreateRoomResData(bool result)
        {
            Result = result;
        }
        public byte[] ToBytes()
        {
            return BitConverter.GetBytes(Result);
        }

        public void FromBytes(byte[] bytes)
        {
            Result = BitConverter.ToBoolean(bytes, 0);
        }
    }
}
