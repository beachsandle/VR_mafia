using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyPacket
{
    public class StartVotingData : IPacketData
    {
        public int Voting_time = 0;
        public int Size
        {
            get
            {
                return 4;
            }
        }

        public StartVotingData(byte[] bytes = null)
        {
            if (bytes != null)
                FromBytes(bytes);
        }

        public StartVotingData(int voting_time)
        {
            Voting_time = voting_time;

        }

        public void FromBytes(byte[] bytes)
        {
            Voting_time = BitConverter.ToInt32(bytes, 0);
        }

        public byte[] ToBytes()
        {
            return BitConverter.GetBytes(Voting_time);
        }
    }
}
