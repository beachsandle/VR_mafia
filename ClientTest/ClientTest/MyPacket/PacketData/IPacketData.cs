using System;
using System.Collections.Generic;
using System.Text;

namespace MyPacket
{
    /// <summary>
    /// 패킷에 저장될 데이터
    /// </summary>
    interface IPacketData
    {
        /// <summary>
        /// 데이터의 크기
        /// </summary>
        int Size { get; }
        /// <summary>
        /// 데이터 직렬화
        /// </summary>
        /// <returns>직렬화된 데이터</returns>
        byte[] ToBytes();
        /// <summary>
        /// 직렬화된 데이터를 읽고 저장
        /// </summary>
        /// <param name="bytes"></param>
        void FromBytes(byte[] bytes);
    }
}
