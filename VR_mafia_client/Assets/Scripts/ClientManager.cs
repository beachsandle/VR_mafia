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

    [HideInInspector]
    public string hostIp;
    [HideInInspector]
    public int port;

    public int playerID;
    public string userName;
    public string roomName;

    public List<UserInfo> users;
    public bool isMafia;

    public static ClientManager instance;
    void Awake()
    {
        instance = this;

        userName = "";
        DontDestroyOnLoad(gameObject);
    }
    private void Start()
    {
        //ConnectToServer();

        users = new List<UserInfo>();
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
    public void ConnectToServer()
    {
        if (socketReady) return;

        if (hostIp == null) hostIp = "127.0.0.1";
        if (port == 0) port = 8080;

        try
        {
            client = new TcpClient(hostIp, port);
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

            if (IntroManager.instance)
            {
                IntroManager.instance.DisplayText("서버와 연결에 실패했습니다.");
            }
        }
    }

    private void OnConnect(Packet packet)
    {
        var data = new ConnectData();
        data.FromBytes(packet.Bytes);

        Debug.Log("OnConnect : " + data.PlayerId);

        playerID = data.PlayerId;
        userName = "Player" + playerID;

        Socket.Clear(PacketType.CONNECT);

        Socket.On(PacketType.SET_NAME_RES, OnSetNameRes);
        Socket.On(PacketType.ROOM_LIST_RES, OnRoomListRes);
        Socket.On(PacketType.CREATE_ROOM_RES, OnCreateRoomRes);

        Socket.On(PacketType.JOIN_ROOM_RES, OnJoinRoomRes);
        Socket.On(PacketType.JOIN_EVENT, OnJoinEvent);
        Socket.On(PacketType.LEAVE_ROOM_RES, OnLeaveRoomRes);
        Socket.On(PacketType.LEAVE_EVENT, OnLeaveEvent);
        Socket.On(PacketType.ROOM_DESTROY_EVENT, OnRoomDestroyEvent);
        Socket.On(PacketType.GAME_START, OnGameStart);

        Socket.On(PacketType.MOVE_EVENT, OnMoveEvent);
        Socket.On(PacketType.DAY_START, OnDayStart);
        Socket.On(PacketType.NIGHT_START, OnNightStart);
        Socket.On(PacketType.START_VOTING, OnStartVoting);
        Socket.On(PacketType.VOTE_RES, OnVoteRes);
        Socket.On(PacketType.VOTING_RESULT, OnVotingResult);
        Socket.On(PacketType.START_DEFENSE, OnStartDefense);
        Socket.On(PacketType.START_FINAL_VOTING, OnStartFinalVoting);
        Socket.On(PacketType.FINAL_VOTE_RES, OnFinalVoteRes);
        Socket.On(PacketType.FINAL_VOTING_RESULT, OnFinalVotingResult);
        Socket.On(PacketType.KILL_RES, OnKillRes);
        Socket.On(PacketType.DIE_EVENT, OnDieEvent);

        SceneManager.LoadScene("Lobby");
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

    #region Lobby
    private void OnSetNameRes(Packet packet)
    {
        var data = new SetNameResData(packet.Bytes);

        Debug.Log("OnSetName : " + data.UserName);

        userName = data.UserName;
    }
    public void EmitSetNameReq(string userName)
    {
        if (!socketReady) return;

        Socket.Emit(PacketType.SET_NAME_REQ, new SetNameReqData(userName).ToBytes());
    }

    private void OnCreateRoomRes(Packet packet)
    {
        var data = new CreateRoomResData(packet.Bytes);
        if (data.Result)
        {
            users.Add(new UserInfo(playerID, userName));
            SceneManager.LoadScene("WaitingRoom");
        }
    }
    public void EmitCreateRoomReq(string roomName)
    {
        if (!socketReady) return;

        this.roomName = roomName;
        Socket.Emit(PacketType.CREATE_ROOM_REQ, new CreateRoomReqData(roomName).ToBytes());
    }

    private void OnJoinRoomRes(Packet packet)
    {
        var data = new JoinRoomResData(packet.Bytes);

        users = data.Users;

        SceneManager.LoadScene("WaitingRoom");
    }
    public void EmitJoinRoomReq(int roomId)
    {
        Socket.Emit(PacketType.JOIN_ROOM_REQ, new JoinRoomReqData(roomId).ToBytes());
    }

    private void OnRoomListRes(Packet packet)
    {
        if (SceneManager.GetActiveScene().name == "InGame") return;

        var data = new RoomListResData(packet.Bytes);

        //TODO: HostId에서 HostName으로 받을 수 있게 변경
        int roomCount = data.Rooms.Count;
        for (int i = 0; i < roomCount; i++)
        {
            var r = data.Rooms[i];
            Debug.Log(r.Name);
            LobbyManager.instance.UpdateRooms(r.Name, r.HostId.ToString(), r.Participants, r.Id);
        }
    }
    public void EmitRoomListReq()
    {
        Socket.Emit(PacketType.ROOM_LIST_REQ);
    }
    #endregion

    #region WaitingRoom
    private void OnJoinEvent(Packet packet)
    {
        var data = new JoinEventData(packet.Bytes);

        users.Add(data.Info);
        WaitingRoomManager.instance.AddPlayer(data.Info);
    }
    private void OnLeaveEvent(Packet packet)
    {
        var data = new LeaveEventData(packet.Bytes);
        users.Remove((from u in users where u.Id == data.PlayerId select u).First());
        WaitingRoomManager.instance.RemovePlayer(data.PlayerId);
    }
    private void OnRoomDestroyEvent(Packet packet)
    {
        SceneManager.LoadScene("Lobby");
    }
    public void EmitLeaveRoomReq()
    {
        Socket.Emit(PacketType.LEAVE_ROOM_REQ);
    }
    private void OnLeaveRoomRes(Packet packet)
    {
        SceneManager.LoadScene("Lobby");
    }

    public void EmitGameStartReq()
    {
        Socket.Emit(PacketType.GAME_START_REQ);
    }
    private void OnGameStart(Packet packet)
    {
        var data = new GameStartData(packet.Bytes);

        isMafia = data.IsMafia;

        Debug.Log("Start");
        SceneManager.LoadScene("InGame");
    }
    #endregion

    #region Ingame
    private void OnMoveEvent(Packet packet)
    {
        var data = new MoveEventData(packet.Bytes);

        InGameManager.instance.UpdatePlayerTransform(data);
    }
    public void EmitMoveReq(Vector3 pos, Quaternion rot)
    {
        if (!socketReady) return;

        Socket.Emit(PacketType.MOVE_REQ, new MoveReqData(MakeLocation(pos, rot)).ToBytes());
    }
    private Location MakeLocation(Vector3 pos, Quaternion rot)
    {
        var position = new V3(pos.x, pos.y, pos.z);
        var rotation = new V3(rot.eulerAngles.x, rot.eulerAngles.y, rot.eulerAngles.z);

        return new Location(position, rotation);
    }

    private void OnDayStart(Packet packet)
    {
        InGameManager.instance.StartDay();
    }
    private void OnNightStart(Packet packet)
    {
        InGameManager.instance.StartNight();
    }

    private void OnStartVoting(Packet packet)
    {
        var data = new StartVotingData(packet.Bytes);

        InGameManager.instance.OnVotingPanel(data.Voting_time);
    }
    public void EmitVoteReq(int targetID)
    {
        Socket.Emit(PacketType.VOTE_REQ, new VoteReqData(targetID).ToBytes());
    }
    private void OnVoteRes(Packet packet)
    {
        var data = new VoteResData(packet.Bytes);

        if (data.Result)
        {
            InGameManager.instance.suffrage = false;
        }
    }
    private void OnVotingResult(Packet packet)
    {
        var data = new VotingResultData(packet.Bytes);

        InGameManager.instance.DisplayVotingResult(data.elected_id, data.result);
    }

    private void OnStartDefense(Packet packet)
    {
        var data = new StartDefenseData(packet.Bytes);

        InGameManager.instance.StartDefense(data.Elected_id, data.Defense_time);
    }

    private void OnStartFinalVoting(Packet packet)
    {
        var data = new StartFinalVotingData(packet.Bytes);

        InGameManager.instance.StartFinalVoting(data.Voting_time);
    }
    public void EmitFinalVoteReq(bool agree)
    {
        Socket.Emit(PacketType.FINAL_VOTE_REQ, new FinalVoteReqData(agree).ToBytes());
    }
    private void OnFinalVoteRes(Packet packet)
    {
        var data = new FinalVoteResData(packet.Bytes);

        if (data.Result)
        {
            InGameManager.instance.suffrage = false;
        }
    }
    private void OnFinalVotingResult(Packet packet)
    {
        var data = new FinalVotingResultData(packet.Bytes);

        InGameManager.instance.DisplayFinalVotingResult(data.Kicking_id, data.voteCount);
    }

    public void EmitKillReq(int targetID)
    {
        Socket.Emit(PacketType.KILL_REQ, new KillReqDada(targetID).ToBytes());
    }
    private void OnKillRes(Packet packet)
    {
        var data = new KillResDada(packet.Bytes);
        
        if (data.Result)
        {
            // 총알 없애기
        }
    }
    private void OnDieEvent(Packet packet)
    {
        var data = new DieEventData(packet.Bytes);

        InGameManager.instance.KillPlayer(data.Dead_id);
    }

    #endregion
}