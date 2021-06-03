using System;
using System.Collections.Generic;
using System.Linq;

namespace MyPacket
{
    public class RoomListResData : IPacketData
    {
        public bool Result = true;
        public List<GameRoomInfo> Rooms = null;
        public int Size
        {
            get
            {
                int size = 1;
                if (Rooms != null)
                {
                    foreach (var r in Rooms)
                        size += r.Size;
                }
                return size;
            }
        }


        public RoomListResData(byte[] bytes = null)
        {
            if (bytes != null)
                FromBytes(bytes);
        }
        public RoomListResData(bool result, List<GameRoomInfo> rooms = null)
        {
            Result = result;
            Rooms = rooms;
        }
        public byte[] ToBytes()
        {
            var bb = new ByteBuilder(Size).Append(Result);
            if (Rooms != null)
            {
                foreach (var room in Rooms)
                {
                    bb.Append(room.ToBytes());
                }
            }
            return bb.Get();
        }

        public void FromBytes(byte[] bytes)
        {
            int idx = 1;
            Result = BitConverter.ToBoolean(bytes, 0);
            Rooms = new List<GameRoomInfo>();
            while (idx < bytes.Length)
            {
                var room = new GameRoomInfo(bytes.Skip(idx).ToArray());
                idx += room.Size;
                Rooms.Add(room);
            }
        }
    }
}
