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
    private void Start()
    {

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

            return false;
        }

        return true;
    }

    private void OnConnect(Packet packet)
    {
        var data = new ConnectData(packet.Bytes);

        Debug.Log("OnConnect : " + data.PlayerId);

        GameManager.Instance.InitPlayerInfo(data.PlayerId);

        Socket.Clear(PacketType.CONNECT);

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
    #endregion

    #region WaitingRoom

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