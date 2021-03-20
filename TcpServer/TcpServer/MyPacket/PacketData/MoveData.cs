using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyPacket
{
    class MoveData : IPacketData
    {
        public int player_id;
        public float[] position;
        public float[] rotation;
        public int Size
        {
            get
            {
                return 28;
            }
        }

        public MoveData()
        {
            player_id = 0;
            position = new float[3] { 0, 0, 0 };
            rotation = new float[3] { 0, 0, 0 };
        }

        public MoveData(int pid, float[] pos, float[] rot)
        {
            player_id = pid;
            position = pos;
            rotation = rot;
        }

        public void FromBytes(byte[] bytes)
        {
            player_id = BitConverter.ToInt32(bytes, 0);
            for (int i = 0; i < 3; ++i)
                position[i] = BitConverter.ToSingle(bytes, 4 + i * 4);
            for (int i = 0; i < 3; ++i)
                rotation[i] = BitConverter.ToSingle(bytes, 16 + i * 4);
        }

        public byte[] ToBytes()
        {
            var bb = new ByteBuilder(28);
            bb.Append(player_id);
            for (int i = 0; i < 3; ++i)
                bb.Append(position[i]);
            for (int i = 0; i < 3; ++i)
                bb.Append(rotation[i]);
            return bb.Get();
        }
    }
}
