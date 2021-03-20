using System;
using System.Collections.Generic;
using System.Text;

namespace MyPacket
{
    public struct LeaveEventData : IPacketData
    {
        public int Size
        {
            get
            {
                return 4;
            }
        }
        public int PlayerId { get; set; }
        public LeaveEventData(int playerId = -1)
        {
            PlayerId = playerId;
        }
        public byte[] ToBytes()
        {
            return BitConverter.GetBytes(PlayerId);
        }
        public void FromBytes(byte[] bytes)
        {
            PlayerId = BitConverter.ToInt32(bytes, 0);
        }
    }
}
