﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyPacket
{
    public class GameRoomInfo
    {
        public int Id { get; set; }
        public int HostId { get; set; }
        public int Participants { get; set; }
        public bool IsStarted { get; set; } = false;
        public string Name { get; set; } = "";
        public int Size
        {
            get
            {
                return sizeof(int) * 4 + 1 + Encoding.UTF8.GetBytes(Name).Length; ;
            }
        }
        public GameRoomInfo() { }
        public GameRoomInfo(int id, int hostId, int num, bool started, string name)
        {
            Id = id;
            HostId = hostId;
            Participants = num;
            Name = name;
            IsStarted = started;
        }
        public byte[] ToBytes()
        {
            return new ByteBuilder(Size)
                .Append(Size)
                .Append(Id)
                .Append(HostId)
                .Append(Participants)
                .Append(IsStarted)
                .Append(Name)
                .Get();
        }
        public void FromBytes(byte[] bytes)
        {
            //size 4칸 무시
            Id = BitConverter.ToInt32(bytes, 4);
            HostId = BitConverter.ToInt32(bytes, 8);
            Participants = BitConverter.ToInt32(bytes, 12);
            IsStarted = BitConverter.ToBoolean(bytes, 16);
            Name = Encoding.UTF8.GetString(bytes, 17, bytes.Length - 17);
        }
    }
}