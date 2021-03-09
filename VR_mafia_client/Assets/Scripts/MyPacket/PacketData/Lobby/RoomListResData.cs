using System;
using System.Collections.Generic;
using System.Text;

namespace MyPacket
{
    struct RoomListResData : IPacketData
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
            var bytes = new byte[Size];
            int idx = 0;
            foreach (var room in Rooms)
            {
                Array.Copy(room.ToBytes(), 0, bytes, idx, room.Size);
                idx += room.Size;
            }
            return bytes;
        }

        public void FromBytes(byte[] bytes)
        {
            int idx = 0;
            Rooms = new List<GameRoomInfo>();
            while (idx < bytes.Length)
            {
                var size = BitConverter.ToInt32(bytes, idx);
                var temp = new byte[size];
                Array.Copy(bytes, idx + 4, temp, 0, size - 4);
                var room = new GameRoomInfo();
                room.FromBytes(temp);
                idx += size;
                Rooms.Add(room);
            }
        }
    }
}
