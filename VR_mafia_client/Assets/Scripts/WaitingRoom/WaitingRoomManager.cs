using MyPacket;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Linq;

public class WaitingRoomManager : MonoBehaviour
{
    private MySocket socket;
    private List<UserInfo> users;

    [Header("Button")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button leaveButton;

    [Header("UI")]
    [SerializeField] private Text roomNameText;

    private Transform[] playerTRs;
    const int HEAD_COUNT = 10;

    void Awake()
    {
        playerTRs = new Transform[HEAD_COUNT];
        var players = GameObject.Find("Players").transform;
        for (int i = 0; i < players.childCount; i++)
        {
            playerTRs[i] = players.GetChild(i);
        }
    }

    void Start()
    {
        socket = ClientManager.Instance.Socket;
        (roomNameText.text, users) = SceneLoader.Instance.GetWaitingRoomInfo();
        for (int i = 0; i < users.Count; i++)
        {
            SetPlayerInfo(i);
        }

        InitWaitingRoomEvent();
        InitButtonListener();
    }
    private void InitButtonListener()
    {
        startButton.onClick.AddListener(OnStartButton);
        leaveButton.onClick.AddListener(OnLeaveButton);
    }
    private void InitWaitingRoomEvent()
    {
        socket.On(PacketType.JOIN_EVENT, OnJoinEvent);
        socket.On(PacketType.LEAVE_ROOM_RES, OnLeaveRes);
        socket.On(PacketType.LEAVE_EVENT, OnLeaveEvent);
        socket.On(PacketType.ROOM_DESTROY_EVENT, OnRoomDestroyEvent);
        socket.On(PacketType.GAME_START, OnGameStart);
    }
    private void ClearWaitingRoomEvent()
    {
        socket.Clear(PacketType.JOIN_EVENT);
        socket.Clear(PacketType.LEAVE_ROOM_RES);
        socket.Clear(PacketType.LEAVE_EVENT);
        socket.Clear(PacketType.ROOM_DESTROY_EVENT);
        socket.Clear(PacketType.GAME_START);
    }
    #region WaitingRoom Event
    private void OnJoinEvent(Packet packet)
    {
        var data = new JoinEventData(packet.Bytes);
        AddPlayer(data.Info);
    }
    private void OnRoomDestroyEvent(Packet packet)
    {
        ClearWaitingRoomEvent();
        SceneLoader.Instance.WaitingRoomToLobby();
    }
    public void EmitLeaveReq()
    {
        socket.Emit(PacketType.LEAVE_ROOM_REQ);
    }
    private void OnLeaveRes(Packet packet)
    {
        // TODO: 나중에 고치기
        var data = new LeaveResData();
        data.FromBytes(packet.Bytes);
        if (data.Result)
        {
            ClearWaitingRoomEvent();
            SceneLoader.Instance.WaitingRoomToLobby();
        }
    }
    private void OnLeaveEvent(Packet packet)
    {
        var data = new LeaveEventData(packet.Bytes);
        RemovePlayer(data.PlayerId);
    }

    public void EmitGameStartReq()
    {
        socket.Emit(PacketType.GAME_START_REQ);
    }
    private void OnGameStart(Packet packet)
    {
        var data = new GameStartData(packet.Bytes);
        Debug.Log("Start");
        ClearWaitingRoomEvent();
        SceneLoader.Instance.WaitingRoomToInGame(data.IsMafia, data.Mafias, users);
    }
    #endregion
    public void AddPlayer(UserInfo user)
    {
        //TODO: SetPlayerInfo()로 교체
        //SetPlayerInfo(ClientManager.instance.users.Count);

        users.Add(user);
        playerTRs[users.Count - 1].Find("Name").GetComponent<Text>().text = user.Name;
        playerTRs[users.Count - 1].Find("Image").GetComponent<Image>().color = Global.colors[users.Count - 1];
    }

    public void RemovePlayer(int playerId)
    {
        users.Remove(users.Find(info => info.Id == playerId));
        // TODO: 플레이어 빈 공간 채워주기
        for (int i = 0; i < users.Count; i++)
        {
            SetPlayerInfo(i);
        }
        for (int i = users.Count; i < HEAD_COUNT; i++)
        {
            playerTRs[i].Find("Name").GetComponent<Text>().text = "P" + (i + 1);
            playerTRs[i].Find("Image").GetComponent<Image>().color = Color.white;
        }
    }

    private void SetPlayerInfo(int playerNum)
    {
        playerTRs[playerNum].Find("Name").GetComponent<Text>().text = users[playerNum].Name;
        playerTRs[playerNum].Find("Image").GetComponent<Image>().color = Global.colors[playerNum];
    }

    void OnStartButton()
    {
        EmitGameStartReq();
    }
    void OnLeaveButton()
    {
        EmitLeaveReq();
    }
}
