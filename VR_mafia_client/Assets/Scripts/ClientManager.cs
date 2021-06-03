using MyPacket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ClientManager : MonoBehaviour
{
    private TcpClient client;
    public MySocket Socket { get; private set; }
    private bool socketReady;

    [HideInInspector] public string hostIp;
    [HideInInspector] public int port;

    public bool isMafia;

    public static ClientManager Instance { get; private set; }
    void Awake()
    {
        Instance = this;

        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        while (socketReady && 0 < Socket.PacketCount)
            Socket.Handle();
    }

    private void OnApplicationQuit()
    {
        CloseSocket();
    }
    public bool ConnectToServer()
    {
        if (socketReady) return true;

        if (hostIp == null) hostIp = "127.0.0.1";
        if (port == 0) port = 8080;

        try
        {
            var client = new Socket(SocketType.Stream, ProtocolType.Tcp);
            client.Connect(hostIp, port);
            Socket = new MySocket(client);
            Socket.On(PacketType.CONNECT, OnConnect);
            Socket.On(PacketType.DISCONNECT, OnDisconnect);
            Socket.Listen(false);
            Socket.Emit(PacketType.CONNECT);
            socketReady = true;
        }
        catch (Exception e)
        {
            Debug.Log("Socket Error" + e.Message);

            return false;
        }

        return true;
    }

    private void OnConnect(Packet packet)
    {
        var data = new ConnectData(packet.Bytes);
        Debug.Log("OnConnect : " + data.PlayerId);

        Socket.Clear(PacketType.CONNECT);

        SceneLoader.Instance.IntroToLobby(data.PlayerId);
    }
    private void OnDisconnect(Packet packet)
    {
        Socket.Close();

        Debug.Log("Disconnect");
    }
    private void CloseSocket()
    {
        if (!socketReady) return;

        Socket.Emit(PacketType.DISCONNECT);
        Socket.Close();

        socketReady = false;
    }
}
