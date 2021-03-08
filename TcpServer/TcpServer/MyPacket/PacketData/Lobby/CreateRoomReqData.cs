using System;
using System.Collections.Generic;
using System.Text;

namespace MyPacket
{
    struct CreateRoomReqData : IPacketData
    {
        public int Size
        {
            get
            {
                return Encoding.UTF8.GetBytes(RoomName).Length;
            }
        }
        public string RoomName { get; set; }
        public CreateRoomReqData(string roomName = "")
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
