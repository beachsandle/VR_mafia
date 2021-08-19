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
    private TypedLobby defaultLobby = new TypedLobby(null, LobbyType.SqlLobby);
    #endregion

    #region callback
    [HideInInspector] public Action<List<RoomInfo>> RoomListChanged;
    [HideInInspector] public Action LeftLobby;
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
        PhotonNetwork.GetCustomRoomList(defaultLobby, "*");
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
    public override void OnJoinedLobby()
    {
        wait = false;
        Debug.Log("[Photon Manager] : joined lobby");
    }
    #endregion

    #region lobby
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        Debug.Log("[Photon Manager] : room list changed");
        RoomListChanged?.Invoke(roomList);
    }
    public override void OnCreatedRoom()
    {
        Debug.Log("[Photon Manager] : room created");
    }
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        wait = false;
        Debug.Log("[Photon Manager] : create failed");
    }
    public override void OnJoinedRoom()
    {
        wait = false;
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
    }
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log($"[Photon Manager] : left user {otherPlayer}");
    }
    #endregion

    #endregion
}
