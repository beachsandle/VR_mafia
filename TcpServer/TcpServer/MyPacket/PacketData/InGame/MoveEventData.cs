using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyPacket
{
    public class MoveEventData : IPacketData
    {
        public (int player_id, Location location)[] movedPlayer = null;
        public int Size
        {
            get
            {
                return 28 * movedPlayer.Length;
            }
        }

        public MoveEventData(byte[] bytes = null)
        {
            if (bytes != null)
                FromBytes(bytes);
        }

        public MoveEventData((int player_id, Location location)[] movedPlayer)
        {
            this.movedPlayer = movedPlayer;
        }

        public void FromBytes(byte[] bytes)
        {
            movedPlayer = new (int, Location)[bytes.Length / 28];
            int idx = 0;
            for (int i = 0; i < movedPlayer.Length; ++i)
            {
                movedPlayer[i].player_id = BitConverter.ToInt32(bytes, idx);
                idx += 4;
                movedPlayer[i].location.FromBytes(bytes.Skip(idx).ToArray());
                idx += 24;
            }
        }

        public byte[] ToBytes()
        {
            var bb = new ByteBuilder(Size);
            foreach (var (player_id, location) in movedPlayer)
            {
                bb.Append(player_id).Append(location.ToBytes());
            }
            return bb.Get();
        }
    }
}
