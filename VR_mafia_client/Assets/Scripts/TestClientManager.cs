using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using MyPacket;

public class TestClientManager : MonoBehaviour
{
	private TcpClient client;
	private MySocket socket;
	private bool socketReady;

	public string hostIp;
	public int port;

	
    private void Start()
    {
		ConnectToServer();
    }
    private void Update()
    {
        if (socketReady)
        {

        }
    }
	private void OnApplicationQuit()
	{
		CloseSocket();
	}

	public void ConnectToServer()
	{
		if (socketReady) return;

		if (hostIp == null) hostIp = "127.0.0.1";
		if (port == 0) port = 8080;

		try
		{
			client = new TcpClient(hostIp, port);
			
			socket = new MySocket(client);
			socket.On(PacketType.CONNECT, OnConnect);
			socket.On(PacketType.DISCONNECT, OnDisconnect);
			socket.On(PacketType.SET_NAME, OnSetName);

			socket.Listen();
			socket.Emit(PacketType.CONNECT);
			socketReady = true;
		}
		catch (Exception e)
		{
			Debug.Log("Socket Error" + e.Message);
		}
	}

	private void OnConnect(MySocket socket, Packet packet)
	{
		//var data = new ConnectData();
		//data.FromBytes(packet.Bytes);
		Debug.Log("OnConnect");
		//PlayerID = data.player_id;
	}
	private void OnDisconnect(MySocket socket, Packet packet)
	{
		socket.Disconnect();
		Debug.Log("Disconnect");
	}
	private void OnSetName(MySocket socket, Packet packet)
	{
		//var data = new SetNameData();
		//data.FromBytes(packet.Bytes);
		Debug.Log("OnSetName");
        //UserName = data.UserName;
        //Dispatcher.Invoke(() => { userNameInput.Text = UserName; });
    }

	private void CloseSocket()
    {
		if (!socketReady) return;

		socket.Emit(PacketType.DISCONNECT);
		socket.Disconnect();

		socketReady = false;
    }
}