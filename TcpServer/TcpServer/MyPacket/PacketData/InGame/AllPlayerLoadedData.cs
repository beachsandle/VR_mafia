using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyPacket
{
    public class AllPlayerLoadedData : IPacketData
    {
        public Tuple<int, string>[] PhotonIdArr;
        public int Size
        {
            get
            {
                return 40 * PhotonIdArr.Length;
            }
        }

        public AllPlayerLoadedData(byte[] bytes = null)
        {
            if (bytes != null)
                FromBytes(bytes);
        }

        public AllPlayerLoadedData(Tuple<int, string>[] photonIdArr)
        {
            PhotonIdArr = photonIdArr;
        }


        public void FromBytes(byte[] bytes)
        {
            PhotonIdArr = new Tuple<int, string>[bytes.Length / 40];
            for (int i = 0; i < PhotonIdArr.Length; ++i)
            {
                int uid = BitConverter.ToInt32(bytes, i * 40);
                string pid = Encoding.UTF8.GetString(bytes, i * 40 + 4, 36);
                PhotonIdArr[i] = Tuple.Create(uid, pid);
            }
        }

        public byte[] ToBytes()
        {
            var bb = new ByteBuilder(Size);
            foreach (var t in PhotonIdArr)
            {
                bb.Append(t.Item1).Append(t.Item2);
            }
            return bb.Get();
        }
    }
}
