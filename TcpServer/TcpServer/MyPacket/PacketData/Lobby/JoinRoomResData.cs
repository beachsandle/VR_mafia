﻿using System;
using System.Collections.Generic;
using System.Text;

namespace MyPacket
{
    public class JoinRoomResData : IPacketData
    {
        public bool Result = true;
        public List<UserInfo> Users = new List<UserInfo>();
        public JoinRoomResData(byte[] bytes = null)
        {
            if (bytes != null)
                FromBytes(bytes);
        }

        public JoinRoomResData(bool result, List<UserInfo> users = null)
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
            var bb = new ByteBuilder(Size);
            bb.Append(Result);
            foreach (var user in Users)
            {
                bb.Append(user.ToBytes());
            }
            return bb.Get();
        }

        public void FromBytes(byte[] bytes)
        {
            Result = BitConverter.ToBoolean(bytes, 0);
            int idx = 1;
            Users = new List<UserInfo>();
            while (idx < bytes.Length)
            {
                var size = BitConverter.ToInt32(bytes, idx);
                var temp = new byte[size - 4];
                Array.Copy(bytes, idx + 4, temp, 0, size - 4);
                var user = new UserInfo();
                user.FromBytes(temp);
                idx += size;
                Users.Add(user);
            }
        }
    }
}
