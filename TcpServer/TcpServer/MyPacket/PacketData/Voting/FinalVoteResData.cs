using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyPacket
{
    public class FinalVoteResData : IPacketData
    {
        public bool Result = true;
        public int Size
        {
            get
            {
                return 1;
            }
        }

        public FinalVoteResData(byte[] bytes = null)
        {
            if (bytes != null)
                FromBytes(bytes);
        }


        public FinalVoteResData(bool result)
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
