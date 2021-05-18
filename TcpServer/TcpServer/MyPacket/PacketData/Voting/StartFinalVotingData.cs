using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyPacket
{
    public class StartFinalVotingData : IPacketData
    {
        public int Voting_time;
        public int Size
        {
            get
            {
                return 4;
            }
        }

        public StartFinalVotingData()
        {
            Voting_time = 30;

        }


        public StartFinalVotingData(int voting_rime)
        {
            Voting_time = voting_rime;


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
