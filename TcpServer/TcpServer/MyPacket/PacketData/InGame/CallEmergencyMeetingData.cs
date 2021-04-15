using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyPacket
{
    public class CallEmergencyMeetingData : IPacketData
    {
        public int User_id;
        public int Size
        {
            get
            {
                return 4;
            }
        }

        public CallEmergencyMeetingData()
        {
            User_id = 0;
        }

        public CallEmergencyMeetingData(int user_id)
        {
            User_id = user_id;
  
        }

        public void FromBytes(byte[] bytes)
        {
            User_id = BitConverter.ToInt32(bytes, 0);
        }

        public byte[] ToBytes()
        {
            return BitConverter.GetBytes(User_id);
        }
    }
}
