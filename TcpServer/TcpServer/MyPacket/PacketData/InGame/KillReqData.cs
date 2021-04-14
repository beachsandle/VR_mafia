using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyPacket
{
    public class KillReqDada : IPacketData
    {
        public int Target_id;
        public int Size
        {
            get
            {
                return 4;
            }
        }

        public KillReqDada()
        {
            Target_id = 0;

        }

        public KillReqDada(int target_id)
        {
            Target_id = target_id;
  
        }

        public void FromBytes(byte[] bytes)
        {
            Target_id = BitConverter.ToInt32(bytes, 0);
        }

        public byte[] ToBytes()
        {
            return BitConverter.GetBytes(Target_id);
        }
    }
}
