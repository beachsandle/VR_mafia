using System;
using System.Collections.Generic;
using System.Text;

namespace MyPacket
{
    struct JoinRoomResData : IPacketData
    {
        public bool Result { get; set; }
        public List<UserInfo> Users { get; set; }

        public JoinRoomResData(bool result = true, List<UserInfo> users = null)
        {
            Result = result;
            Users = users;
        }
        public int Size
        {
            get
            {
                int size = 1;
                foreach (var u in Users)
                    size += u.Size;
                return size;
            }
        }

        public byte[] ToBytes()
        {
            var bytes = new byte[Size];
            Array.Copy(BitConverter.GetBytes(Result), bytes, 1);
            int idx = 1;
            foreach (var user in Users)
            {
                Array.Copy(user.ToBytes(), 0, bytes, idx, user.Size);
                idx += user.Size;
            }
            return bytes;
        }

        public void FromBytes(byte[] bytes)
        {
            Result = BitConverter.ToBoolean(bytes, 0);
            int idx = 1;
            Users = new List<UserInfo>();
            while (idx < bytes.Length)
            {
                var size = BitConverter.ToInt32(bytes, idx);
                var temp = new byte[size];
                Array.Copy(bytes, idx + 4, temp, 0, size - 4);
                var user = new UserInfo();
                user.FromBytes(temp);
                idx += size;
                Users.Add(user);
            }
        }
    }
}
