using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyPacket
{
    public class StartDefenseData: IPacketData
    {
        public int Defense_time;
        public int Elected_id;
        public int Size
        {
            get
            {
                return 8;
            }
        }

        public StartDefenseData()
        {
            Defense_time = 20; // 임의로 20초 초기화
            Elected_id = 0;
        }

        public StartDefenseData(int defense_time, int elected_id)
        {
            Defense_time = defense_time;
            Elected_id = elected_id;

        }


        public void FromBytes(byte[] bytes)
        {
            Defense_time = BitConverter.ToInt32(bytes, 0);
            Elected_id = BitConverter.ToInt32(bytes, 1);
        }

        public byte[] ToBytes()
        {
            var bb = new ByteBuilder(Size);
            bb.Append(Defense_time);
            bb.Append(Elected_id);

            return bb.Get();
        }
    }
}
