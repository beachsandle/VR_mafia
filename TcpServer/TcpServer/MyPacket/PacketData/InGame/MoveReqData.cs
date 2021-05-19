using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyPacket
{
    public class MoveReqData : IPacketData
    {
        public Location location = new Location();
        public int Size
        {
            get
            {
                return 24;
            }
        }

        public MoveReqData(byte[] bytes = null)
        {
            if (bytes != null)
                FromBytes(bytes);
        }

        public MoveReqData(Location trans)
        {
            location = trans;
        }

        public void FromBytes(byte[] bytes)
        {
            location.FromBytes(bytes.ToArray());
        }

        public byte[] ToBytes()
        {
            return new ByteBuilder(28).Append(location.ToBytes()).Get();
        }
    }
}
