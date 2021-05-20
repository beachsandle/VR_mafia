using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyPacket
{
    public class VotingResultData : IPacketData
    {
        public (int id, int count)[] result = null;
        public int Size
        {
            get
            {
                return result?.Length * 8 ?? 0;
            }
        }

        public VotingResultData(byte[] bytes = null)
        {
            if (bytes != null)
                FromBytes(bytes);
        }


        public VotingResultData((int, int)[] result)
        {
            this.result = result;

        }

        public void FromBytes(byte[] bytes)
        {
            result = new (int, int)[bytes.Length / 8];
            for (int i = 0; i < result.Length; ++i)
            {
                result[i].id = BitConverter.ToInt32(bytes, i * 8);
                result[i].count = BitConverter.ToInt32(bytes, i * 8 + 4);
            }
        }

        public byte[] ToBytes()
        {
            var bb = new ByteBuilder(Size);
            foreach ((int id, int count) in result)
            {
                bb.Append(id).Append(count);
            }
            return bb.Get();
        }
    }
}
