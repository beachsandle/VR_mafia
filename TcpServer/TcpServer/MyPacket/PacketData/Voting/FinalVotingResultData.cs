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
        public int voteCount = 0;
        public int Size
        {
            get
            {
                return 8;
            }
        }

        public FinalVotingResultData(byte[] bytes = null)
        {
            if (bytes != null)
                FromBytes(bytes);
        }


        public FinalVotingResultData(int kicking_id, int voteCount = 0)
        {
            Kicking_id = kicking_id;
            this.voteCount = voteCount;

        }

        public void FromBytes(byte[] bytes)
        {
            Kicking_id = BitConverter.ToInt32(bytes, 0);
            voteCount = BitConverter.ToInt32(bytes, 4);
        }

        public byte[] ToBytes()
        {
            return new ByteBuilder(8).Append(Kicking_id).Append(voteCount).Get();
        }
    }
}
