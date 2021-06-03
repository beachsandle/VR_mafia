using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyPacket
{
    public class JoinEventData : IPacketData
    {
        public UserInfo Info = null;
        public int Size
        {
            get
            {
                return Info.Size;
            }
        }
        public JoinEventData(byte[] bytes = null)
        {
            if (bytes != null)
                FromBytes(bytes);
        }
        public JoinEventData(UserInfo info)
        {
            Info = info;
        }
        public byte[] ToBytes()
        {
            return Info.ToBytes();
        }
        public void FromBytes(byte[] bytes)
        {
            Info = new UserInfo(bytes);
        }
    }
}
