using System;
using System.Collections.Generic;
using System.Text;

namespace MyPacket
{
    /// <summary>
    /// 패킷의 헤더
    /// </summary>
    [Serializable]
    public class PacketHeader
    {
        public static readonly int SIZE = 8;
        public PacketType Type { get; set; }
        public int Size { get; set; }
        public bool HasData
        {
            get
            {
                return Size != 0;
            }
        }
        public PacketHeader(byte[] bytes = null)
        {
            if (bytes != null)
                FromBytes(bytes);
        }
        public PacketHeader(PacketType type, int size)
        {
            Type = type;
            Size = size;
        }
        public byte[] GetBytes()
        {
            var result = new byte[SIZE];
            BitConverter.GetBytes((int)Type).CopyTo(result, 0);
            BitConverter.GetBytes(Size).CopyTo(result, 4);
            return result;
        }
        public void FromBytes(byte[] bytes)
        {
            Type = (PacketType)BitConverter.ToInt32(bytes, 0);
            Size = BitConverter.ToInt32(bytes, 4);
        }
        public override string ToString()
        {
            return $"type : {Type}, size : {Size}";
        }
    }
}
