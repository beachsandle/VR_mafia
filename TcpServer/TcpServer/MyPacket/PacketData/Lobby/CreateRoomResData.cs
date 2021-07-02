using System;
using System.Collections.Generic;
using System.Text;

namespace MyPacket
{
    public class CreateRoomResData : IPacketData
    {
        public bool Result = true;
        public int RoomId = -1;
        public int Size
        {
            get
            {
                return 5;
            }
        }
        public CreateRoomResData(byte[] bytes = null)
        {
            if (bytes != null)
                FromBytes(bytes);
        }
        public CreateRoomResData(bool result, int roomId)
        {
            Result = result;
            RoomId = roomId;
        }
        public byte[] ToBytes()
        {
            return new ByteBuilder(5).Append(Result).Append(RoomId).Get();
        }

        public void FromBytes(byte[] bytes)
        {
            Result = BitConverter.ToBoolean(bytes, 0);
            RoomId = BitConverter.ToInt32(bytes, 1);
        }
    }
}
