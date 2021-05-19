using System;
using System.Collections.Generic;
using System.Text;

namespace MyPacket
{
    public class ConnectData : IPacketData
    {
        public int PlayerId = -1;
        public int Size
        {
            get
            {
                return 4;
            }
        }
        public ConnectData(byte[] bytes = null)
        {
            if (bytes != null)
                FromBytes(bytes);
        }
        public ConnectData(int playerId)
        {
            PlayerId = playerId;
        }
        public byte[] ToBytes()
        {
            return BitConverter.GetBytes(PlayerId);
        }
        public void FromBytes(byte[] bytes)
        {
            if (bytes.Length != 0)
                PlayerId = BitConverter.ToInt32(bytes, 0);
        }
    }
}
