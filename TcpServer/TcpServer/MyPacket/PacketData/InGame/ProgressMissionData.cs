using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyPacket
{
    public class ProgressMissionData : IPacketData
    {
        public float Progress;
        public int Size
        {
            get
            {
                return 4;
            }
        }

        public ProgressMissionData()
        {
            Progress = 0;

        }

        public ProgressMissionData(int progress)
        {
            Progress = progress;
        }

        public void FromBytes(byte[] bytes)
        {
            Progress = BitConverter.ToInt32(bytes, 0);
        }

        public byte[] ToBytes()
        {
            return BitConverter.GetBytes(Progress);
        }
    }
}
