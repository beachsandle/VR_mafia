using System;
using System.Collections.Generic;
using System.Text;

namespace MyPacket
{
    struct Packet
    {
        public PacketHeader Header { get; private set; }
        public byte[] Bytes { get; private set; }
        public int Size
        {
            get
            {
                return PacketHeader.SIZE + Header.Size;
            }
        }
        public Packet(PacketType type = PacketType.NONE, byte[] bytes = null)
        {
            Bytes = bytes;
            Header = new PacketHeader(type, Bytes?.Length ?? 0);
        }
        public Packet(PacketHeader header, byte[] bytes = null)
        {
            Header = header;
            Bytes = new byte[header.Size];
            if (Header.HasData)
                Array.Copy(bytes, Bytes, Header.Size);
        }
        public byte[] ToBytes()
        {
            var result = new byte[PacketHeader.SIZE + Header.Size];
            Header.GetBytes().CopyTo(result, 0);
            if (Header.HasData)
                Bytes.CopyTo(result, PacketHeader.SIZE);
            return result;
        }
    }
}
