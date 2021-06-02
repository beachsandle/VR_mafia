using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Linq;

namespace MyPacket
{
    public class MySocket
    {
        #region delegate
        public delegate void MessageHandler(Packet packet);
        #endregion
        #region field
        protected Socket socket;
        protected Queue<Packet> readQueue = new Queue<Packet>();
        protected Dictionary<PacketType, MessageHandler> handlerMap = new Dictionary<PacketType, MessageHandler>();
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
        public MySocket(Socket socket)
        {
            this.socket = socket;
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
        //열거자에서 패킷을 읽고 패킷의 크기를 반환
        private int ReadPacket(IEnumerable<byte> buffer)
        {
            var header = new PacketHeader(buffer.Take(8).ToArray());
            var bytes = header.HasData ?
                buffer.Skip(8).Take(header.Size).ToArray() : null;
            var packet = new Packet(header, bytes);
            if (IsAsync)
                handlerMap[header.Type](packet);
            else
                readQueue.Enqueue(packet);
            return packet.Size;
        }
        //비동기 핸들러
        private void OnRecieved(object sender, SocketAsyncEventArgs e)
        {
            var target = e.Buffer.Skip(e.Offset).Take(e.BytesTransferred);
            for (int i = e.Offset; i < e.BytesTransferred;)
            {
                int cnt = ReadPacket(target);
                target.Skip(cnt);
                i += cnt;
            }
            if (e.BytesTransferred != 0)
                (sender as Socket).ReceiveAsync(e);
            else
                Close();
        }
        //공백 핸들러
        private void EmptyHandler(Packet packet) { }
        #endregion
        #region public method
        //on disconnect 호출
        public void Close()
        {
            handlerMap[PacketType.DISCONNECT](new Packet());
            socket.Close();
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
            var packet = new Packet(type, bytes);
            socket.Send(packet.ToBytes());
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
            var e = new SocketAsyncEventArgs();
            e.SetBuffer(new byte[1024], 0, 1024);
            e.Completed += OnRecieved;
            e.UserToken = this;
            socket.ReceiveAsync(e);
        }
        #endregion
    }
}
