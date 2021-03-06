﻿using MyPacket;
using UnityEngine;
using UnityEngine.UI;

public class LobbyManager : MonoBehaviour
{
    private MySocket socket;
    private static LobbyManager _instance = null;
    public static LobbyManager instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<LobbyManager>();
            }
            return _instance;
        }
    }
    private string userName;
    private int roomId;
    private string roomName;
    public RoomList roomList;

    [Header("UI")]
    [SerializeField] private Text playerName;

    [Header("Button")]
    [SerializeField] private Button joinButton;
    [SerializeField] private Button createButton;
    [SerializeField] private Button refreshButton;
    [SerializeField] private Button changeNameButton;

    [Header("ChangeName Panel")]
    [SerializeField] private GameObject changeNamePanel;
    [SerializeField] private InputField nameInputField;

    [Header("CreateRoom Panel")]
    [SerializeField] private GameObject createRoomPanel;
    [SerializeField] private InputField roomNameInputField;

    private void Start()
    {
        socket = ClientManager.Instance.Socket;

        InitLobbyEvent();
        InitPlayerName();
        InitButtonListener();
        InitChangeNamePanel();
        InitCreateRoomPanel();

        EmitRoomListReq();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (changeNamePanel.activeSelf)
            {
                OnChangeNameOKButton();
            }
            else if (createRoomPanel.activeSelf)
            {
                OnCreateRoomOKButton();
            }
        }
    }

    private void InitPlayerName()
    {
        userName = SceneLoader.Instance.UserName;
        playerName.text = userName;
        EmitSetNameReq(userName);
    }
    private void InitLobbyEvent()
    {
        socket.On(PacketType.SET_NAME_RES, OnSetNameRes);
        socket.On(PacketType.ROOM_LIST_RES, OnRoomListRes);
        socket.On(PacketType.CREATE_ROOM_RES, OnCreateRoomRes);
        socket.On(PacketType.JOIN_ROOM_RES, OnJoinRoomRes);
    }
    private void ClearLobbyEvent()
    {
        socket.Clear(PacketType.SET_NAME_RES);
        socket.Clear(PacketType.ROOM_LIST_RES);
        socket.Clear(PacketType.CREATE_ROOM_RES);
        socket.Clear(PacketType.JOIN_ROOM_RES);
    }

    #region Lobby Event
    private void EmitSetNameReq(string userName)
    {
        socket.Emit(PacketType.SET_NAME_REQ, new SetNameReqData(userName));
    }
    private void OnSetNameRes(Packet packet)
    {
        var data = new SetNameResData(packet.Bytes);
        if (data.Result)
        {
            Debug.Log("OnSetName : " + data.UserName);
            userName = data.UserName;
            playerName.text = userName;
        }
    }

    public void EmitRoomListReq()
    {
        socket.Emit(PacketType.ROOM_LIST_REQ);
    }
    private void OnRoomListRes(Packet packet)
    {
        var data = new RoomListResData(packet.Bytes);

        roomList.Clear();
        for (int i = 0; i < data.Rooms.Count; i++)
        {
            var r = data.Rooms[i];

            Debug.Log(r.Name);
            roomList.CreateRoom(r.Name, r.Host.Name, r.Participants, r.Id);
        }
    }

    public void EmitCreateRoomReq()
    {
        socket.Emit(PacketType.CREATE_ROOM_REQ, new CreateRoomReqData(roomName));
    }
    private void OnCreateRoomRes(Packet packet)
    {
        var data = new CreateRoomResData(packet.Bytes);
        if (data.Result)
        {
            ClearLobbyEvent();
            SceneLoader.Instance.LobbyToWaitingRoom(userName, data.RoomId, roomName);
        }
    }

    public void EmitJoinRoomReq(int roomId)
    {
        this.roomId = roomId;
        socket.Emit(PacketType.JOIN_ROOM_REQ, new JoinRoomReqData(roomId));
    }
    private void OnJoinRoomRes(Packet packet)
    {
        var data = new JoinRoomResData(packet.Bytes);
        if (data.Result)
        {
            ClearLobbyEvent();
            SceneLoader.Instance.LobbyToWaitingRoom(userName, roomId, roomList.select.name, data.Users);
        }
    }
    #endregion

    private void InitButtonListener()
    {
        createButton.onClick.AddListener(OnCreateRoomButton);
        joinButton.onClick.AddListener(OnJoinButton);
        refreshButton.onClick.AddListener(OnRefreshButton);
        changeNameButton.onClick.AddListener(OnChangeNameButton);
    }

    void OnJoinButton()
    {
        EmitJoinRoomReq(roomList.select.id);
    }

    void OnRefreshButton()
    {
        EmitRoomListReq();
    }

    #region ChangeNamePanel
    void InitChangeNamePanel()
    {
        changeNamePanel.SetActive(true);

        changeNamePanel.transform.GetChild(0).Find("OK Button").GetComponent<Button>().onClick.AddListener(OnChangeNameOKButton);
        changeNamePanel.transform.GetChild(0).Find("NO Button").GetComponent<Button>().onClick.AddListener(OnChangeNameNOButton);
        nameInputField.characterLimit = 10;

        changeNamePanel.SetActive(false);
    }
    void OnChangeNameButton()
    {
        nameInputField.text = playerName.text;
        changeNamePanel.SetActive(true);
    }
    void OnChangeNameOKButton()
    {
        EmitSetNameReq(nameInputField.text);
        changeNamePanel.SetActive(false);
    }
    void OnChangeNameNOButton()
    {
        changeNamePanel.SetActive(false);
    }
    #endregion

    #region CreateRoomPanel
    void InitCreateRoomPanel()
    {
        createRoomPanel.SetActive(true);
        var content = createRoomPanel.transform.GetChild(0);
        content.Find("OK Button").GetComponent<Button>().onClick.AddListener(OnCreateRoomOKButton);
        content.Find("NO Button").GetComponent<Button>().onClick.AddListener(OnCreateRoomNOButton);
        nameInputField.characterLimit = 10;

        createRoomPanel.SetActive(false);
    }
    void OnCreateRoomButton()
    {
        createRoomPanel.SetActive(true);
        roomNameInputField.text = "";
    }
    void OnCreateRoomOKButton()
    {
        roomName = roomNameInputField.text;
        EmitCreateRoomReq();
        createRoomPanel.SetActive(false);
    }
    void OnCreateRoomNOButton()
    {
        createRoomPanel.SetActive(false);
    }
    #endregion
}
