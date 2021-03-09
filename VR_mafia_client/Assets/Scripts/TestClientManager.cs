using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using MyPacket;

public class TestClientManager : MonoBehaviour
{
	private static TestClientManager _instance;
	public static TestClientManager instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = FindObjectOfType<TestClientManager>();
			}

			return _instance;
		}
	}

	private TcpClient client;
	private MySocket socket;
	private bool socketReady;

	public string hostIp;
	public int port;

	private int playerID;

    private void Start()
    {
		ConnectToServer();
    }
    private void Update()
    {
        //if (socketReady)
        //{
			
        //}
    }

	private float[] Vector3ToFloatArr(Vector3 v)
    {
		return new float[] { v.x, v.y, v.z };
    }
	private Vector3 FloatArrToVector3(float[] f)
    {
		return new Vector3(f[0], f[1], f[2]);
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
			socket.On(PacketType.MOVE, OnMove);

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
        var data = new ConnectData();
        data.FromBytes(packet.Bytes);
        Debug.Log("OnConnect : " + data.PlayerId);
        playerID = data.PlayerId;
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

	private void OnMove(MySocket socket, Packet packet)
    {
		var data = new MoveData();
		data.FromBytes(packet.Bytes);
		Debug.Log(FloatArrToVector3(data.position));
    }
	public void EmitMove(Vector3 pos, Quaternion rot)
    {
		if (!socketReady) return;
		
		socket.Emit(PacketType.MOVE, new MoveData(playerID, Vector3ToFloatArr(pos), Vector3ToFloatArr(rot.eulerAngles)).ToBytes());
	}

	private void CloseSocket()
    {
		if (!socketReady) return;

		socket.Emit(PacketType.DISCONNECT);
		socket.Disconnect();

		socketReady = false;
    }
}