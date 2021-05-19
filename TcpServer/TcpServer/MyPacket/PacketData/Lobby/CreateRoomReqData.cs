using System;
using System.Collections.Generic;
using System.Text;

namespace MyPacket
{
    public class CreateRoomReqData : IPacketData
    {
        public string RoomName = "";
        public int Size
        {
            get
            {
                return Encoding.UTF8.GetBytes(RoomName).Length;
            }
        }
        public CreateRoomReqData(byte[] bytes = null)
        {
            if (bytes != null)
                FromBytes(bytes);
        }
        public CreateRoomReqData(string roomName)
        {
            RoomName = roomName;
        }
        public byte[] ToBytes()
        {
            return Encoding.UTF8.GetBytes(RoomName);
        }

        public void FromBytes(byte[] bytes)
        {
            RoomName = Encoding.UTF8.GetString(bytes);
        }
    }
}
