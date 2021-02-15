using System.Net;
using System.Net.Sockets;
using MyPacket;

namespace TcpServer
{
    class Program
    {
        static void Main(string[] args)
        {
            var tcpServer = new TcpListener(IPAddress.Any, 8080);
            var server = new GameServer(tcpServer);
            server.Start();
        }
    }
}
