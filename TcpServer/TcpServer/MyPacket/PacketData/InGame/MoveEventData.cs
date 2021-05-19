using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyPacket
{
    public class MoveEventData : IPacketData
    {
        public int Player_id = -1;
        public Location Location = new Location();
        public int Size
        {
            get
            {
                return 28;
            }
        }

        public MoveEventData(byte[] bytes = null)
        {
            if (bytes != null)
                FromBytes(bytes);
        }

        public MoveEventData(int player_id, Location location)
        {
            Player_id = player_id;
            Location = location;
        }

        public void FromBytes(byte[] bytes)
        {
            Player_id = BitConverter.ToInt32(bytes, 0);
            Location.FromBytes(bytes.Skip(4).ToArray());
        }

        public byte[] ToBytes()
        {
            return new ByteBuilder(28).Append(Player_id).Append(Location.ToBytes()).Get();
        }
    }
}
