using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyPacket
{
    public class DieEventData : IPacketData
    {
        public int Dead_id;
        public int Size
        {
            get
            {
                return 4;
            }
        }

        public DieEventData()
        {
            Dead_id = 0;

        }

        public DieEventData(int dead_id)
        {
            Dead_id = dead_id;
  
        }

        public void FromBytes(byte[] bytes)
        {
            Dead_id = BitConverter.ToInt32(bytes, 0);
        }

        public byte[] ToBytes()
        {
            return BitConverter.GetBytes(Dead_id);
        }
    }
}
