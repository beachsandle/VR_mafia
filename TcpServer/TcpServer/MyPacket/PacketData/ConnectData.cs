using System;
using System.Collections.Generic;
using System.Text;

namespace MyPacket
{
    struct ConnectData : IPacketData
    {
        private const int size = sizeof(int);
        public int player_id;
        public int Size
        {
            get
            {
                return size;
            }
        }
        public ConnectData(int pid = -1)
        {
            player_id = pid;
        }
        public byte[] ToBytes()
        {
            return BitConverter.GetBytes(player_id);
        }
        public void FromBytes(byte[] bytes)
        {
            if (bytes.Length != 0)
                player_id = BitConverter.ToInt32(bytes, 0);
        }
    }
}
