using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyPacket
{
    public class StartVotingData : IPacketData
    {
        public int Voting_time;
        public int Size
        {
            get
            {
                return 4;
            }
        }

        public StartVotingData()
        {
            Voting_time = 60;//그냥 임의로 60초로 둠
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
