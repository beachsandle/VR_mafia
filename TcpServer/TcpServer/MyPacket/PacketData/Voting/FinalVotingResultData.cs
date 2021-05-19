using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyPacket
{
    public class FinalVotingResultData : IPacketData
    {
        public int Kicking_id = -1;
        public int Size
        {
            get
            {
                return 4;
            }
        }

        public FinalVotingResultData(byte[] bytes = null)
        {
            if (bytes != null)
                FromBytes(bytes);
        }


        public FinalVotingResultData(int kicking_id)
        {
            Kicking_id = kicking_id;

        }

        public void FromBytes(byte[] bytes)
        {
            Kicking_id = BitConverter.ToInt32(bytes, 0);
        }

        public byte[] ToBytes()
        {
            return BitConverter.GetBytes(Kicking_id);
        }
    }
}
