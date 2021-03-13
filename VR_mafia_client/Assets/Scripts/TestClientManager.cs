﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using MyPacket;

public class TestClientManager : MonoBehaviour
{
    private TcpClient client;
    public MySocket socket;
    private bool socketReady;

    public string hostIp;
    public int port;

    public int playerID;
    public string userName;

    public List<UserInfo> users;

    public static TestClientManager instance;
    void Awake()
    {
        instance = this;

        DontDestroyOnLoad(gameObject);
    }
    private void Start()
    {
        ConnectToServer();

        users = new List<UserInfo>();
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
            socket = new GameObject("socket").AddComponent<MySocket>();
            socket.Init(client);
            socket.On(PacketType.CONNECT, OnConnect);
            socket.On(PacketType.DISCONNECT, OnDisconnect);
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
        socket.Clear(PacketType.CONNECT);
        socket.On(PacketType.SET_NAME, OnSetName);
        socket.On(PacketType.ROOM_LIST_RES, OnRoomListRes);
        socket.On(PacketType.CREATE_ROOM_RES, OnCreateRoomRes);
        socket.On(PacketType.JOIN_ROOM_RES, OnJoinRoomRes);
        socket.On(PacketType.JOIN_EVENT, OnJoinEvent);
        socket.On(PacketType.GAME_START, OnGameStart);
        socket.Emit(PacketType.ROOM_LIST_REQ);
    }
    private void OnDisconnect(MySocket socket, Packet packet)
    {
        socket.Disconnect();
        Debug.Log("Disconnect");
    }

    #region Lobby
    private void OnSetName(MySocket socket, Packet packet)
    {
        var data = new SetNameData();
        data.FromBytes(packet.Bytes);
        Debug.Log("OnSetName");

        userName = data.UserName;
    }

    private void OnCreateRoomRes(MySocket socket, Packet packet)
    {
        users.Add(new UserInfo(playerID, userName));
        SceneManager.LoadScene("WaitingRoom");
    }
    public void EmitCreateRoomReq(string roomName)
    {
        if (!socketReady) return;

        socket.Emit(PacketType.CREATE_ROOM_REQ, new CreateRoomReqData(roomName).ToBytes());
    }

    private void OnJoinRoomRes(MySocket socket, Packet packet)
    {
        var data = new JoinRoomResData();
        data.FromBytes(packet.Bytes);
        users = data.Users;
        Debug.Log("join res");
        SceneManager.LoadScene("WaitingRoom");
    }
    public void EmitJoinRoomReq(int roomId)
    {
        socket.Emit(PacketType.JOIN_ROOM_REQ, new JoinRoomReqData(roomId).ToBytes());
    }

    private void OnRoomListRes(MySocket socket, Packet packet)
    {
        if (SceneManager.GetActiveScene().name == "InGame") return;

        Debug.Log("OnRoomListRes");
        var data = new RoomListResData();
        data.FromBytes(packet.Bytes);

        int roomCount = data.Rooms.Count;
        for (int i = 0; i < roomCount; i++)
        {
            var r = data.Rooms[i];
            LobbyManager.instance.UpdateRooms(r.Name, r.HostId.ToString(), r.Participants, r.Id);
        }
    }
    public void EmitRoomListReq()
    {
        socket.Emit(PacketType.ROOM_LIST_REQ);
    }
    #endregion

    #region WaitingRoom
    private void OnJoinEvent(MySocket socket, Packet packet)
    {
        var data = new JoinEventData();
        data.FromBytes(packet.Bytes);

        //TODO: Name 값이 비어있음
        //Debug.Log("Join : " + data.Info.Name);

        WaitingRoomManager.instance.AddPlayer(data.Info);
    }

    public void EmitGameStartReq()
    {
        socket.Emit(PacketType.GAME_START_REQ);
    }
    private void OnGameStart(MySocket socket, Packet packet)
    {
        Debug.Log("Start");

        SceneManager.LoadScene("InGame");
    }
    #endregion

    #region Ingame
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
    #endregion

    private void CloseSocket()
    {
        if (!socketReady) return;

        socket.Emit(PacketType.DISCONNECT);
        socket.Disconnect();

        socketReady = false;
    }
}