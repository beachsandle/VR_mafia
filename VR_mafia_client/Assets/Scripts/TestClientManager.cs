using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Text;
using System.Net.Sockets;
using System.Threading;

public class TestClientManager : MonoBehaviour
{
	[Serializable]
	enum PacketType
	{
		NONE,
		CONNECT,
		DISCONNECT,
		SET_NAME,
		ROOM_LIST_REQ,
		MOVE,
		END
	}

	private TcpClient socket;
	private NetworkStream stream;
	private StreamWriter writer;
	private StreamReader reader;
	private bool socketReady;

	public string hostIp;
	public int port;

	
    private void Start()
    {
		ConnectToServer();
    }
    private void Update()
    {
        if(socketReady && stream.DataAvailable)
        {
			string data = reader.ReadLine();
			if(data != null)
            {
				Debug.Log(data);
				Send("받았음.");
            }
        }
    }
	public void ConnectToServer()
	{
		if (socketReady) return;

		if (hostIp == null) hostIp = "127.0.0.1";
		if (port == 0) port = 8080;

		try
		{
			socket = new TcpClient(hostIp, port);
			stream = socket.GetStream();
			writer = new StreamWriter(stream);
			reader = new StreamReader(stream);
			socketReady = true;
			Emit();
		}
		catch (Exception e)
		{
			Debug.Log("Socket Error" + e.Message);
		}
	}
	void Send(string data)
    {
		if (!socketReady) return;

		writer.WriteLine(data);
		writer.Flush();
    }

    private void OnApplicationQuit()
    {
		CloseSocket();
    }
	void CloseSocket()
    {
		if (!socketReady) return;

		writer.Close();
		reader.Close();
		socket.Close();
		socketReady = false;
    }

	public void Emit()
	{
		var result = new byte[8];
		BitConverter.GetBytes(2).CopyTo(result, 0);
		BitConverter.GetBytes(0).CopyTo(result, 4);

		stream.Write(result, 0, 8);
	}
}