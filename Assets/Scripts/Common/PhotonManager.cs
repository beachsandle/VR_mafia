using System;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Dissonance;
using Dissonance.Audio.Playback;
using Dissonance.Integrations.PhotonUnityNetworking2;
using UnityEngine.SceneManagement;

public class PhotonManager : MonoBehaviourPunCallbacks
{
    #region singleton
    private static PhotonManager instance;
    public static PhotonManager Instance
    {
        get
        {
            if (instance == null)
                instance = FindObjectOfType<PhotonManager>();
            return instance;
        }
    }
    #endregion

    #region field
    private bool wait = false;
    private Player host;
    private TypedLobby defaultLobby = new TypedLobby(null, LobbyType.SqlLobby);
    public static Color[] colors = {
        Color.red, Color.green,
        Color.blue, Color.cyan,
        Color.magenta, Color.yellow,
        Color.gray, Color.black,
        new Color(0.75f, 0.5f, 0.75f), new Color(0.5f, 0.75f, 0.5f)
    };
    #endregion

    #region property
    private Photon.Realtime.Room cr => PhotonNetwork.CurrentRoom;
    public Player[] PlayerList => PhotonNetwork.PlayerList;
    #endregion

    #region callback

    #region lobby
    public event Action<List<RoomInfo>> RoomListChanged;
    public event Action LeftLobby;
    #endregion

    #region waiting room
    public event Action PlayerListChanged;
    public event Action LeftWaitingRoom;
    #endregion

    #endregion

    #region unity message
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
    #endregion

    #region method

    #region intro
    public void Connect(string nickname)
    {
        if (wait) return;
        wait = true;
        try
        {
            // #Critical, we must first and foremost connect to Photon Online Server.
            PhotonNetwork.GameVersion = "1.0";
            PhotonNetwork.NickName = nickname;
            PhotonNetwork.ConnectUsingSettings();
        }
        catch
        {
            wait = false;
        }
    }
    #endregion

    #region lobby
    public void JoinLobby()
    {
        if (wait) return;
        wait = true;
        try
        {
            PhotonNetwork.JoinLobby(defaultLobby);
        }
        catch
        {
            wait = false;
        }
    }
    public void CreateRoom(string roomName)
    {
        if (wait) return;
        wait = true;
        var option = new RoomOptions();
        option.MaxPlayers = 10;
        option.CustomRoomProperties = new ExitGames.Client.Photon.Hashtable() { { "HostName", PhotonNetwork.NickName } };
        option.CustomRoomPropertiesForLobby = new string[] { "HostName" };
        try
        {
            PhotonNetwork.CreateRoom(roomName, option);
        }
        catch
        {
            wait = false;
        }
    }
    public void RefreshRoomList()
    {
        //PhotonNetwork.GetCustomRoomList(defaultLobby, "*");
    }
    public void JoinRoom(RoomInfo roomInfo)
    {
        if (wait) return;
        wait = true;
        try
        {
            PhotonNetwork.JoinRoom(roomInfo.Name);
        }
        catch
        {
            wait = false;
        }
    }
    #endregion

    #region waiting room

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
        LeftWaitingRoom?.Invoke();
    }
    public void GameStart()
    {
        if (!PhotonNetwork.LocalPlayer.IsMasterClient)
            return;
        cr.IsOpen = false;
        LeftWaitingRoom?.Invoke();
    }
    #endregion

    #endregion

    #region event handler

    #region intro
    public override void OnConnectedToMaster()
    {
        wait = false;
        Debug.Log("[Photon Manager] : connected");
        SceneManager.LoadScene("Lobby");
    }
    #endregion

    #region lobby
    public override void OnJoinedLobby()
    {
        wait = false;
        Debug.Log("[Photon Manager] : joined lobby");
    }
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        Debug.Log("[Photon Manager] : room list changed");
        RoomListChanged?.Invoke(roomList);
    }
    public override void OnCreatedRoom()
    {
        Debug.Log("[Photon Manager] : room created");
        PhotonNetwork.CurrentRoom.SetMasterClient(PhotonNetwork.LocalPlayer);
    }
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        wait = false;
        Debug.Log("[Photon Manager] : create failed");
    }
    public override void OnJoinedRoom()
    {
        wait = false;
        host = cr.Players[cr.MasterClientId];
        Debug.Log("[Photon Manager] : joined room");
        LeftLobby?.Invoke();
        SceneManager.LoadScene("WaitingRoom");
    }
    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        wait = false;
        Debug.Log("[Photon Manager] : join failed");
    }
    #endregion

    #region waitting room
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log($"[Photon Manager] : enter user {newPlayer}");
        PlayerListChanged?.Invoke();
    }
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log($"[Photon Manager] : left user {otherPlayer}");
        if (otherPlayer == host)
            LeaveRoom();
        else
            PlayerListChanged?.Invoke();
    }
    #endregion

    #endregion
}
