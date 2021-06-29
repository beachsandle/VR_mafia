using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyPacket
{
    public class GameEndData : IPacketData
    {
        public bool MafiaWin = false;
        public int[] Winner = new int[0];
        public int Size
        {
            get
            {
                return 1 + Winner.Length * 4;
            }
        }

        public GameEndData(byte[] bytes = null)
        {
            if (bytes != null)
                FromBytes(bytes);
        }

        public GameEndData(bool mafiaWin, int[] winner)
        {
            MafiaWin = mafiaWin;
            Winner = winner;
        }

        public void FromBytes(byte[] bytes)
        {
            MafiaWin = BitConverter.ToBoolean(bytes, 0);
            Winner = new int[bytes.Length / 4];
            for (int i = 0; i < Winner.Length; ++i)
            {
                Winner[i] = BitConverter.ToInt32(bytes, 1 + i * 4);
            }
        }

        public byte[] ToBytes()
        {
            var bb = new ByteBuilder(Size).Append(MafiaWin);
            foreach (int id in Winner)
                bb.Append(id);
            return bb.Get();
        }
    }
}
