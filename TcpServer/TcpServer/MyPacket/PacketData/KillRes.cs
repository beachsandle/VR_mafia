using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyPacket
{
    public class KillRes : IPacketData
    {
        public bool Result;
        public int Size
        {
            get
            {
                return 1;
            }
        }

        public KillRes()
        {
            Result = 0;

        }

        public KillRes(int result)
        {
            Result = result;
  
        }

        public void FromBytes(byte[] bytes)
        {
            Result = BitConverter.ToBoolean(bytes, 0);
  
        }

        public byte[] ToBytes()
        {
            return BitConverter.GetBytes(Result);
        }
    }
}
