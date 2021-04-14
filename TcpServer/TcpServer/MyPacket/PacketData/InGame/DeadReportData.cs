using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyPacket
{
    public class DeadReportData : IPacketData
    {
        public int Dead_id;
        public int Reporter_id;
        public int Size
        {
            get
            {
                return 8;
            }
        }

        public DeadReportData()
        {
            Dead_id = 0;
            Reporter_id = 0;
        }

        public DeadReportData(int dead_id, int reporter_id)
        {
            Dead_id = dead_id;
            Reporter_id = reporter_id
  
        }


        public void FromBytes(byte[] bytes)
        {
            Dead_id = BitConverter.ToInt32(bytes, 0);
            Reporter_id = BitConverter.ToInt32(bytes, 1);
        }

        public byte[] ToBytes()
        {
            var bb = new ByteBuilder(Size);
            bb.Append(Dead_id);
            bb.Append(Reporter_id);

            return bb.Get();
        }
    }
}
