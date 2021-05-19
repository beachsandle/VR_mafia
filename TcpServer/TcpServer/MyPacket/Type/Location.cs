using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyPacket
{
    public struct Location
    {
        public V3 position, rotation;
        public Location(V3 position, V3 rotation)
        {
            this.position = position;
            this.rotation = rotation;
        }
        public byte[] ToBytes()
        {
            return new ByteBuilder(24).Append(position.ToBytes()).Append(rotation.ToBytes()).Get();
        }
        public void FromBytes(byte[] bytes)
        {
            position.FromBytes(bytes.Take(12).ToArray());
            rotation.FromBytes(bytes.Skip(12).ToArray());
        }
        public bool Equals(Location loc)
        {
            return position.Equals(loc.position) && rotation.Equals(loc.rotation);
        }
    }
}
