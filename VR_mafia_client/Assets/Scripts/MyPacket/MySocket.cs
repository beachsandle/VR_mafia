using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

namespace MyPacket
{
    class MySocket : MonoBehaviour
    {
        #region delegate
        public delegate void MessageHandler(MySocket socket, Packet packet);
        #endregion
        #region field
        protected TcpClient client;
        protected NetworkStream stream;
        protected PacketHeader header;
        protected byte[] buffer;
        protected Dictionary<PacketType, MessageHandler> handler;
        #endregion
        #region constructor
        public void Init(TcpClient client)
        {
            this.client = client;
            stream = client.GetStream();
            buffer = new byte[1024];
            HandlerInit();
        }
        #endregion
        #region public method
        /// <summary>
        /// 클라이언트와 연결 해제
        /// </summary>
        public void Disconnect()
        {
            Emit(PacketType.DISCONNECT);
            Close();
        }
        /// <summary>
        /// 소켓 연결 해제
        /// </summary>
        public void Close()
        {
            stream.Close();
            client.Close();
        }
        /// <summary>
        /// 이벤트 핸들러 등록
        /// </summary>
        /// <param name="type"></param>
        /// <param name="handler"></param>
        public void On(PacketType type, MessageHandler handler)
        {
            this.handler[type] += handler;
        }
        /// <summary>
        /// 특정 종류 패킷의 핸들러 제거
        /// </summary>
        /// <param name="type"></param>
        /// <param name="handler"></param>
        public void Off(PacketType type, MessageHandler handler)
        {
            this.handler[type] -= handler;
        }
        /// <summary>
        /// 특정 종류 패킷의 핸들러 초기화
        /// </summary>
        /// <param name="type"></param>
        public void Clear(PacketType type)
        {
            this.handler[type] = EmptyHandler;
        }
        /// <summary>
        /// 특정 종류의 패킷과 해당하는 데이터를 소켓에 전송
        /// </summary>
        /// <param name="type"></param>
        /// <param name="data"></param>
        public void Emit(PacketType type, byte[] bytes = null)
        {
            var packet = new Packet(type, bytes);
            lock (stream)
            {
                stream.Write(packet.ToBytes(), 0, packet.Size);
            }
        }
        /// <summary>
        /// 비동기 방식으로 메시지 처리 시작
        /// </summary>
        public virtual void Listen()
        {
            var thread = new Thread(new ThreadStart(ListenMessage));
            thread.Start();
        }
        #endregion
        #region private method
        /// <summary>
        /// 모든 패킷 종류에 대한 핸들러를 공백 핸들러로 초기화
        /// </summary>
        private void HandlerInit()
        {
            handler = new Dictionary<PacketType, MessageHandler>();
            foreach (PacketType type in Enum.GetValues(typeof(PacketType)))
            {
                handler[type] = EmptyHandler;
            }
        }
        /// <summary>
        /// 클라이언트가 연결되어 있는 동안 패킷을 읽고 처리
        /// </summary>
        private void ListenMessage()
        {
            while (client.Connected)
            {
                if (stream.CanRead && stream.DataAvailable)
                {
                    lock (stream)
                    {
                        ReadPacket();
                        eventQ.Enqueue(new Packet(header, buffer));
                    }
                }
            }
        }
        private Queue<Packet> eventQ = new Queue<Packet>();
        private void Update()
        {
            while (eventQ.Count != 0)
            {
                var packet = eventQ.Dequeue();
                handler[packet.Header.Type](this, packet);
            }
        }
        /// <summary>
        /// 스트림에서 패킷을 읽고 버퍼에 저장
        /// </summary>
        private void ReadPacket()
        {
            stream.Read(buffer, 0, PacketHeader.SIZE);
            header.FromBytes(buffer);
            if (buffer.Length < header.Size)
                buffer = new byte[buffer.Length * 2];
            if (header.Size != 0)
                stream.Read(buffer, 0, header.Size);
        }
        /// <summary>
        /// 공백 핸들러
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="data"></param>
        /// 
        private void EmptyHandler(MySocket socket, Packet packet) { }
        #endregion
    }
}
