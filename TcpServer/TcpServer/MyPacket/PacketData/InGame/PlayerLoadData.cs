using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyPacket
{
    public class PlayerLoadData : IPacketData
    {
        public string PhotonId = "00000000-0000-0000-0000-000000000000";
        public int Size
        {
            get
            {
                return 36;
            }
        }

        public PlayerLoadData(byte[] bytes = null)
        {
            if (bytes != null)
                FromBytes(bytes);
        }

        public PlayerLoadData(string photonId)
        {
            PhotonId = photonId;
        }


        public void FromBytes(byte[] bytes)
        {
            PhotonId = Encoding.UTF8.GetString(bytes);
        }

        public byte[] ToBytes()
        {
            return Encoding.UTF8.GetBytes(PhotonId);
        }
    }
}
