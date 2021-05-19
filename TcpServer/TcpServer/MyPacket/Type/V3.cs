using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyPacket
{
    public struct V3
    {
        public static V3 zero
        {
            get
            {
                return new V3();
            }
        }

        public float x, y, z;
        public V3(float x = 0, float y = 0, float z = 0)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
        public byte[] ToBytes()
        {
            return new ByteBuilder(12).Append(x).Append(y).Append(z).Get();
        }
        public void FromBytes(byte[] bytes)
        {
            x = BitConverter.ToSingle(bytes, 0);
            y = BitConverter.ToSingle(bytes, 4);
            z = BitConverter.ToSingle(bytes, 8);
        }

        public bool Equals(V3 v3)
        {
            return Math.Abs(x - v3.x) < 0.0001f &&
                Math.Abs(y - v3.y) < 0.0001f &&
                Math.Abs(z - v3.z) < 0.0001f;
        }
        public override string ToString()
        {
            return $"({x}, {y}, {z})";
        }
    }
}
