using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyPacket
{
    public class GameStartData : IPacketData
    {
        public int Size
        {
            get
            {
                if (!IsMafia)
                    return 1;
                return 1 + Mafias.Length * 4;
            }
        }
        public bool IsMafia { get; set; }
        public int[] Mafias { get; set; }
        public GameStartData(bool isMafia = false, int[] mafias = null)
        {
            IsMafia = isMafia;
            Mafias = mafias;
        }
        public byte[] ToBytes()
        {
            var bb = new ByteBuilder(Size);
            bb.Append(IsMafia);
            if (IsMafia)
            {
                foreach (int i in Mafias)
                    bb.Append(i);
            }
            return bb.Get();
        }
        public void FromBytes(byte[] bytes)
        {
            IsMafia = BitConverter.ToBoolean(bytes, 0);
            Mafias = null;
            if (IsMafia)
            {
                var mafias = new List<int>();
                for (int i = 1; i < bytes.Length; i += 4)
                    mafias.Add(BitConverter.ToInt32(bytes, i));
                Mafias = mafias.ToArray();
            }
        }
    }
}
