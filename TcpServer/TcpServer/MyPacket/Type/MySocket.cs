using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;

namespace MyPacket
{
    public class MySocket
    {
        #region delegate
        public delegate void MessageHandler(Packet packet);
        #endregion
        #region field
        protected TcpClient client;
        protected NetworkStream stream;
        protected Dictionary<PacketType, MessageHandler> handlerMap = new Dictionary<PacketType, MessageHandler>();
        protected Queue<Packet> readQueue = new Queue<Packet>();
        protected byte[] buffer;
        #endregion
        #region property
        public bool IsAsync { get; set; }
        public int PacketCount
        {
            get
            {
                return readQueue.Count;
            }
        }
        #endregion
        #region constructor
        public MySocket(TcpClient client)
        {
            this.client = client;
            stream = client.GetStream();
            stream.Flush();
            buffer = new byte[1024];
            InitHandlerMap();
        }
        #endregion
        #region private method
        //모든 패킷 종류에 대한 핸들러를 공백 핸들러로 초기화
        private void InitHandlerMap()
        {
            foreach (PacketType type in Enum.GetValues(typeof(PacketType)))
            {
                handlerMap[type] = EmptyHandler;
            }
        }
        //클라이언트와 연결이 종료되기 전까지 패킷을 읽고 처리 또는 저장
        private void ReadMessage()
        {
            try
            {
                while (stream.CanRead)
                {
                    ReadPacket();
                    Thread.Sleep(1);
                }
            }
            //연결이 종료될 경우 disconnect 핸들러 호출
            catch (System.IO.IOException)
            {
                handlerMap[PacketType.DISCONNECT](new Packet());
            }
        }
        //스트림에서 패킷을 읽고 버퍼에 저장
        private void ReadPacket()
        {
            var header = new PacketHeader();
            //먼저 헤더 크기만큼 읽고 버퍼에 저장
            stream.Read(buffer, 0, PacketHeader.SIZE);
            header.FromBytes(buffer);

            //버퍼의 크기가 데이터의 크기보다 작으면 확장
            if (buffer.Length < header.Size)
                buffer = new byte[buffer.Length * 2];

            //데이터 크기만큼 읽고 버퍼에 저장
            if (header.Size != 0)
                stream.Read(buffer, 0, header.Size);

            //비동기적인 경우 바로 핸들러를 호출하고 아닐 경우 큐에 저장
            var packet = new Packet(header, buffer);
            if (IsAsync)
                handlerMap[header.Type](packet);
            else
                readQueue.Enqueue(packet);
        }
        //공백 핸들러
        private void EmptyHandler(Packet packet) { }
        #endregion
        #region public method
        //클라이언트와 스트림의 연결을 해제
        public void Close()
        {
            stream.Close();
            client.Close();
        }
        //이벤트 핸들러 등록
        public void On(PacketType type, MessageHandler handler)
        {
            handlerMap[type] += handler;
        }
        //이벤트 핸들러 제거
        public void Off(PacketType type, MessageHandler handler)
        {
            handlerMap[type] -= handler;
        }
        //이벤트 핸들러 초기화
        public void Clear(PacketType type)
        {
            handlerMap[type] = EmptyHandler;
        }
        //메시지 전송
        public void Emit(PacketType type, byte[] bytes = null)
        {
            if (stream.CanWrite)
            {
                var packet = new Packet(type, bytes);
                stream.Write(packet.ToBytes(), 0, packet.Size);
            }
            else
                handlerMap[PacketType.DISCONNECT](new Packet());
        }
        //readQueue에서 메시지 하나를 읽고 처리
        public void Handle()
        {
            if (readQueue.Count != 0)
            {
                var packet = readQueue.Dequeue();
                handlerMap[packet.Header.Type](packet);
            }
        }
        //메시지 처리 시작
        public void Listen(bool isAsync)
        {
            IsAsync = isAsync;
            var readThread = new Thread(ReadMessage);
            readThread.Start();
        }
        #endregion
    }
}
