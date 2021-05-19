using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyPacket
{
    public class VoteResData : IPacketData
    {
        public bool Result = true;
        public int Size
        {
            get
            {
                return 4;
            }
        }

        public VoteResData(byte[] bytes = null)
        {
            if (bytes != null)
                FromBytes(bytes);
        }


        public VoteResData(bool result)
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
