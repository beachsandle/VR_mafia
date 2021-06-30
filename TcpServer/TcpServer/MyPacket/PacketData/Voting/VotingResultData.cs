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
        public Tuple<int, int>[] result = null;
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


        public VotingResultData(int id, Tuple<int, int>[] result)
        {
            elected_id = id;
            this.result = result;

        }

        public void FromBytes(byte[] bytes)
        {
            elected_id = BitConverter.ToInt32(bytes, 0);
            result = new Tuple<int, int>[bytes.Length / 8];
            for (int i = 0; i < result.Length; ++i)
            {
                int id = BitConverter.ToInt32(bytes, i * 8 + 4);
                int count = BitConverter.ToInt32(bytes, i * 8 + 8);
                result[i] = Tuple.Create(id, count);
            }
        }

        public byte[] ToBytes()
        {
            var bb = new ByteBuilder(Size).Append(elected_id);
            foreach (var t in result)
            {
                bb.Append(t.Item1).Append(t.Item2);
            }
            return bb.Get();
        }
    }
}
