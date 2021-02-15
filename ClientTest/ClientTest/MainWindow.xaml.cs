using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Net.Sockets;
using MyPacket;

namespace ClientTest
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        MySocket socket;
        TextBox userNameInput;
        int PlayerID;
        string UserName;
        public MainWindow()
        {
            InitializeComponent();
            userNameInput = FindName("UserNameInput") as TextBox;

            var client = new TcpClient("127.0.0.1", 8080);
            socket = new MySocket(client);
            socket.On(PacketType.CONNECT, OnConnect);
            socket.On(PacketType.DISCONNECT, OnDisconnect);
            socket.On(PacketType.SET_NAME, OnSetName);
            socket.Listen();
            MessageBox.Show("Connect req");
            socket.Emit(PacketType.CONNECT);
        }
        private void OnConnect(MySocket socket, Packet packet)
        {
            var data = new ConnectData();
            data.FromBytes(packet.Bytes);
            PlayerID = data.player_id;
            MessageBox.Show($"connect res : {data.player_id}");
        }
        private void OnDisconnect(MySocket socket, Packet packet)
        {
            MessageBox.Show("disconnect");
            socket.Disconnect();
        }
        private void OnSetName(MySocket socket, Packet packet)
        {
            var data = new SetNameData();
            data.FromBytes(packet.Bytes);
            UserName = data.UserName;
            Dispatcher.Invoke(() => { userNameInput.Text = UserName; });
            MessageBox.Show($"My name is {data.UserName}");
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            UserName = userNameInput.Text;
            socket.Emit(PacketType.SET_NAME, new SetNameData(UserName).ToBytes());
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            socket.Emit(PacketType.DISCONNECT);
            socket.Disconnect();
        }

    }

}
