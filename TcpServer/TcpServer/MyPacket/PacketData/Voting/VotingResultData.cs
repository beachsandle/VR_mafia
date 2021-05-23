using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyPacket
{
    public class VotingResultData : IPacketData
    {
        public int elected_id = -1;
        public (int id, int count)[] result = null;
        public int Size
        {
            get
            {
                return 4 + result?.Length * 8 ?? 0;
            }
        }

        public VotingResultData(byte[] bytes = null)
        {
            if (bytes != null)
                FromBytes(bytes);
        }


        public VotingResultData(int id, (int, int)[] result)
        {
            elected_id = id;
            this.result = result;

        }

        public void FromBytes(byte[] bytes)
        {
            elected_id = BitConverter.ToInt32(bytes, 0);
            result = new (int, int)[bytes.Length / 8];
            for (int i = 0; i < result.Length; ++i)
            {
                result[i].id = BitConverter.ToInt32(bytes, i * 8 + 4);
                result[i].count = BitConverter.ToInt32(bytes, i * 8 + 8);
            }
        }

        public byte[] ToBytes()
        {
            var bb = new ByteBuilder(Size).Append(elected_id);
            foreach ((int id, int count) in result)
            {
                bb.Append(id).Append(count);
            }
            return bb.Get();
        }
    }
}
