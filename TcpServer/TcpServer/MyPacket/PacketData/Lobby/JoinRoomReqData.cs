using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyPacket
{
    public class JoinRoomReqData : IPacketData
    {
        public int Size
        {
            get
            {
                return 4;
            }
        }
        public int RoomId { get; set; }
        public JoinRoomReqData(int roomId = -1)
        {
            RoomId = roomId;
        }
        public byte[] ToBytes()
        {
            return BitConverter.GetBytes(RoomId);
        }
        public void FromBytes(byte[] bytes)
        {
            RoomId = BitConverter.ToInt32(bytes, 0);
        }
    }
}
