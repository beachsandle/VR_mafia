using System;
using System.Collections.Generic;
using System.Text;

namespace MyPacket
{
    public struct CreateRoomResData : IPacketData
    {
        public int Size
        {
            get
            {
                return 1;
            }
        }
        public bool Result { get; set; }
        public CreateRoomResData(bool result = true)
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
