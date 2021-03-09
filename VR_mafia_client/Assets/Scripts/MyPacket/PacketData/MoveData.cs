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
            var bytes = new byte[28];
            Array.Copy(BitConverter.GetBytes(player_id), bytes, 4);
            for (int i = 0; i < 3; ++i)
                Array.Copy(BitConverter.GetBytes(position[i]), 0, bytes, 4 + i * 4, 4);
            for (int i = 0; i < 3; ++i)
                Array.Copy(BitConverter.GetBytes(rotation[i]), 0, bytes, 16 + i * 4, 4);
            return bytes;
        }
    }
}
