using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyPacket
{
    public class VoteEventData : IPacketData
    {
        public int Voter_id;
        public int Size
        {
            get
            {
                return 4;
            }
        }

        public VoteEventData()
        {
            Voter_id = 0;
        }

        public VoteEventData(int voter_id)
        {
            Voter_id = voter_id;
        }

        public void FromBytes(byte[] bytes)
        {
            Voter_id = BitConverter.ToInt32(bytes, 0);
        }

        public byte[] ToBytes()
        {
            return BitConverter.GetBytes(Voter_id);
        }
    }
}
