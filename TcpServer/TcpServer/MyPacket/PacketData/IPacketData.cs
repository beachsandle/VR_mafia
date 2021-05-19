using System;
using System.Collections.Generic;
using System.Text;

namespace MyPacket
{
    /// 패킷에 저장될 데이터
    interface IPacketData
    {
        // 데이터의 크기
        int Size { get; }
        // 데이터 직렬화
        byte[] ToBytes();
        // 직렬화된 데이터를 읽고 저장
        void FromBytes(byte[] bytes);
        /*
        public packetdata(byte[] bytes = null)
        {
            if (bytes != null)
                FromBytes(bytes);
        }
         */
    }
}
