using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyPacket
{
    public class JoinEventData : IPacketData
    {
        public int Size
        {
            get
            {
                return Info.Size;
            }
        }
        public UserInfo Info { get; set; }
        public JoinEventData(UserInfo info = null)
        {
            Info = info;
        }
        public byte[] ToBytes()
        {
            return Info.ToBytes();
        }
        public void FromBytes(byte[] bytes)
        {
            Info = new UserInfo();
            Info.FromBytes(bytes);
        }
    }
}
