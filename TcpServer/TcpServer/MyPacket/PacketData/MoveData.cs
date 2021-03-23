using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyPacket
{
    public class MoveData : IPacketData
    {
        public int player_id;
        public Location location;
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
            location = new Location();
        }

        public MoveData(int pid, Location trans)
        {
            player_id = pid;
            location = trans;
        }

        public void FromBytes(byte[] bytes)
        {
            player_id = BitConverter.ToInt32(bytes, 0);
            location.FromBytes(bytes.Skip(4).ToArray());
        }

        public byte[] ToBytes()
        {
            return new ByteBuilder(28).Append(player_id).Append(location.ToBytes()).Get();
        }
    }
}
