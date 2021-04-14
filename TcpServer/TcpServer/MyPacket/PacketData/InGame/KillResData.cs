using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyPacket
{
    public class KillResDada : IPacketData
    {
        public bool Result;
        public int Size
        {
            get
            {
                return 1;
            }
        }

        public KillResDada()
        {
            Result = true;

        }

        public KillResDada(bool result)
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
