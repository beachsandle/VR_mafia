using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
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

    [HideInInspector]
    public string hostIp;
    [HideInInspector]
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
        //ConnectToServer();

        users = new List<UserInfo>();
    }
    private void Update()
    {
        while (socketReady && 0 < socket.MessageCount)
            socket.Handle();
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
            socket.Listen(false);
            socket.Emit(PacketType.CONNECT);
            socketReady = true;

            SceneManager.LoadScene("Lobby");
        }
        catch (Exception e)
        {
            Debug.Log("Socket Error" + e.Message);

            if (IntroManager.instance)
            {
                IntroManager.instance.DisplayText("서버와 연결에 실패했습니다.");
            }
        }
    }

    private void OnConnect(MySocket socket, Packet packet)
    {
        var data = new ConnectData();
        data.FromBytes(packet.Bytes);
        Debug.Log("OnConnect : " + data.PlayerId);
        playerID = data.PlayerId;
        socket.Clear(PacketType.CONNECT);
        socket.On(PacketType.SET_NAME_RES, OnSetNameRes);
        socket.On(PacketType.ROOM_LIST_RES, OnRoomListRes);
        socket.On(PacketType.CREATE_ROOM_RES, OnCreateRoomRes);
        socket.On(PacketType.JOIN_ROOM_RES, OnJoinRoomRes);
        socket.On(PacketType.JOIN_EVENT, OnJoinEvent);
        socket.On(PacketType.GAME_START, OnGameStart);
        socket.On(PacketType.LEAVE_ROOM_RES, OnLeaveRoomRes);
        socket.On(PacketType.LEAVE_EVENT, OnLeaveEvent);
        socket.On(PacketType.MOVE, OnMove);
        socket.Emit(PacketType.ROOM_LIST_REQ);
    }
    private void OnDisconnect(MySocket socket, Packet packet)
    {
        socket.Disconnect();
        Debug.Log("Disconnect");
    }

    #region Lobby
    private void OnSetNameRes(MySocket socket, Packet packet)
    {
        var data = new SetNameResData();
        data.FromBytes(packet.Bytes);

        Debug.Log("OnSetName : " + data.UserName);

        userName = data.UserName;
    }
    public void EmitSetName(string userName)
    {
        if (!socketReady) return;

        socket.Emit(PacketType.SET_NAME_REQ, new SetNameReqData(userName).ToBytes());
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

        SceneManager.LoadScene("WaitingRoom");
    }
    public void EmitJoinRoomReq(int roomId)
    {
        socket.Emit(PacketType.JOIN_ROOM_REQ, new JoinRoomReqData(roomId).ToBytes());
    }

    private void OnRoomListRes(MySocket socket, Packet packet)
    {
        if (SceneManager.GetActiveScene().name == "InGame") return;

        var data = new RoomListResData();
        data.FromBytes(packet.Bytes);

        //TODO: HostId에서 HostName으로 받을 수 있게 변경
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

        users.Add(data.Info);
        WaitingRoomManager.instance.AddPlayer(data.Info);
    }
    private void OnLeaveEvent(MySocket socket, Packet packet)
    {
        var data = new LeaveEventData();
        data.FromBytes(packet.Bytes);
        if (data.PlayerId == playerID)
        {
            SceneManager.LoadScene("Lobby");
        }
        else
        {
            users.Remove((from u in users where u.Id == data.PlayerId select u).First());
            WaitingRoomManager.instance.RemovePlayer(data.PlayerId);
        }
    }

    public void EmitLeaveRoomReq()
    {
        socket.Emit(PacketType.LEAVE_ROOM_REQ);
    }
    private void OnLeaveRoomRes(MySocket socket, Packet packet)
    {
        SceneManager.LoadScene("Lobby");
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

        //Debug.Log(data.location.position + ", " + data.location.position);
        InGameManager.instance.UpdatePlayerTransform(data);
    }
    public void EmitMove(Vector3 pos, Quaternion rot)
    {
        if (!socketReady) return;

        socket.Emit(PacketType.MOVE, new MoveData(playerID, MakeLocation(pos, rot)).ToBytes());
    }
    private Location MakeLocation(Vector3 pos, Quaternion rot)
    {
        var position = new V3(pos.x, pos.y, pos.z);
        var rotation = new V3(rot.eulerAngles.x, rot.eulerAngles.y, rot.eulerAngles.z);
        return new Location(position, rotation);
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