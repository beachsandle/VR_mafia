using System;
using System.Collections.Generic;
using System.Text;

namespace MyPacket
{
    public struct RegisterMission : IPacketData
    {
 
        public List<MissionInfo> Missions { get; set; }

        public RegisterMission(List<MissionInfo> missions = null)
        {

            Missions = missions;
        }

        public int Size
        {
            get
            {
                foreach (var u in Missions)
                    size += u.Size;
                return size;
            }
        }

        public byte[] ToBytes()
        {
            var bb = new ByteBuilder(Size);
            foreach (var mission in Missions)
            {
                bb.Append(mission.ToBytes());
            }
            return bb.Get();
        }


        public void FromBytes(byte[] bytes)
        {
            int idx = 0;
            Missions = new List<MissionInfo>();
            while (idx < bytes.Length)
            {
                var size = BitConverter.ToInt32(bytes, idx);
                var temp = new byte[size - 4];
                Array.Copy(bytes, idx + 4, temp, 0, size - 4);
                var mission = new MissionInfo();
                room.FromBytes(temp);
                idx += size;
                Missions.Add(mission);
            }
        }
    }
    }
}
