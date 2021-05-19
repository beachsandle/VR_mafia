using System;
using System.Collections.Generic;
using System.Text;

namespace MyPacket
{
    public class LeaveEventData : IPacketData
    {
        public int PlayerId = -1;
        public int Size
        {
            get
            {
                return 4;
            }
        }
        public LeaveEventData(byte[] bytes = null)
        {
            if (bytes != null)
                FromBytes(bytes);
        }
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
