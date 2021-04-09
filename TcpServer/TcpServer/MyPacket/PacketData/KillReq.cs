using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyPacket
{
    public class KillReq : IPacketData
    {
        public int Target_id;
        public int Size
        {
            get
            {
                return 4;
            }
        }

        public KillReq()
        {
            Target_id = 0;

        }

        public KillReq(int target_id)
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
