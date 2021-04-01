using System;
using System.Collections.Generic;
using System.Text;

namespace MyPacket
{
    public struct RoomListResData : IPacketData
    {
        public List<GameRoomInfo> Rooms { get; set; }

        public RoomListResData(List<GameRoomInfo> rooms = null)
        {
            Rooms = rooms;
        }
        public int Size
        {
            get
            {
                int size = 0;
                foreach (var r in Rooms)
                    size += r.Size;
                return size;
            }
        }

        public byte[] ToBytes()
        {
            var bb = new ByteBuilder(Size);
            foreach (var room in Rooms)
            {
                bb.Append(room.ToBytes());
            }
            return bb.Get();
        }

        public void FromBytes(byte[] bytes)
        {
            int idx = 0;
            Rooms = new List<GameRoomInfo>();
            while (idx < bytes.Length)
            {
                var size = BitConverter.ToInt32(bytes, idx);
                var temp = new byte[size - 4];
                Array.Copy(bytes, idx + 4, temp, 0, size - 4);
                var room = new GameRoomInfo();
                room.FromBytes(temp);
                idx += size;
                Rooms.Add(room);
            }
        }
    }
}
