using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyPacket
{
    public class FinishMissionData : IPacketData
    {
        public int Mission_id;
        public int User_id;

        public int Size
        {
            get
            {
                return 8;
            }
        }
        public FinishMissionData()
        {
            Mission_id = 0;
            User_id = 0;
        }

        public FinishMissionData(int mission_id, int user_id)
        {
            Mission_id = mission_id;
            User_id = user_id;
        }


        public void FromBytes(byte[] bytes)
        {
            Mission_id = BitConverter.ToInt32(bytes, 0);
            User_id = BitConverter.ToInt32(bytes, 1);
        }

        public byte[] ToBytes()
        {
            var bb = new ByteBuilder(Size);
            bb.Append(Mission_id);
            bb.Append(User_id);

            return bb.Get();
        }
    }
}
