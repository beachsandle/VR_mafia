using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyPacket
{
    public class VoteResData : IPacketData
    {
        public int Result;
        public int Size
        {
            get
            {
                return 4;
            }
        }

        public VoteResData()
        {
            Result = 0;

        }


        public VoteResData(int result)
        {
            Result = result;
  
        }

        public void FromBytes(byte[] bytes)
        {
            Result = BitConverter.ToInt32(bytes, 0);
        }

        public byte[] ToBytes()
        {
            return BitConverter.GetBytes(Result);
        }
    }
}
