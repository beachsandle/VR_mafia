using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyPacket
{
    public class ByteBuilder
    {
        private byte[] bytes;
        private int cursor = 0;
        public ByteBuilder(int size)
        {
            bytes = new byte[size];
        }
        public ByteBuilder Append(int val)
        {
            Array.Copy(BitConverter.GetBytes(val), 0, bytes, cursor, 4);
            cursor += 4;
            return this;
        }
        public ByteBuilder Append(float val)
        {
            Array.Copy(BitConverter.GetBytes(val), 0, bytes, cursor, 4);
            cursor += 4;
            return this;
        }
        public ByteBuilder Append(bool val)
        {
            Array.Copy(BitConverter.GetBytes(val), 0, bytes, cursor, 4);
            cursor += 4;
            return this;
        }
        public ByteBuilder Append(string val)
        {
            var encoded = Encoding.UTF8.GetBytes(val);
            Array.Copy(encoded, 0, bytes, cursor, encoded.Length);
            cursor += encoded.Length;
            return this;
        }
        public ByteBuilder Append(byte[] bytes)
        {
            Array.Copy(bytes, 0, this.bytes, cursor, bytes.Length);
            cursor += bytes.Length;
            return this;
        }
        public byte[] Get()
        {
            return bytes;
        }
    }
}
