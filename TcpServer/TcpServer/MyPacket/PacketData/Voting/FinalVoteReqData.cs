using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyPacket
{
    public class FinalVoteReq : IPacketData
    {
        public bool Agree = true;
        public int Size
        {
            get
            {
                return 1;
            }
        }

        public FinalVoteReq(byte[] bytes = null)
        {
            if (bytes != null)
                FromBytes(bytes);
        }

        public FinalVoteReq(bool agree)
        {
            Agree = agree;

        }

        public byte[] ToBytes()
        {
            return BitConverter.GetBytes(Agree);
        }

        public void FromBytes(byte[] bytes)
        {
            Agree = BitConverter.ToBoolean(bytes, 0);
        }
    }
}
