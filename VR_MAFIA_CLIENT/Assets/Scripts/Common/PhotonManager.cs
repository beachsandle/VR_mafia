using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

using Hashtable = ExitGames.Client.Photon.Hashtable;
using static Photon.Pun.PhotonNetwork;

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
    private readonly TypedLobby defaultLobby = new TypedLobby(null, LobbyType.SqlLobby);
    private static readonly RaiseEventOptions toMasterOption = new RaiseEventOptions() { Receivers = ReceiverGroup.MasterClient };
    private static readonly RaiseEventOptions multicastOption = new RaiseEventOptions() { Receivers = ReceiverGroup.Others };
    private static readonly RaiseEventOptions broadcastOption = new RaiseEventOptions() { Receivers = ReceiverGroup.All };
    #endregion

    #region property
    private Photon.Realtime.Room cr => PhotonNetwork.CurrentRoom;
    public Player[] PlayerList => PhotonNetwork.PlayerList;
    #endregion

    #region unity message
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
    #endregion

    #region method
    public static void SendEventToMaster(VrMafiaEventCode code, object content)
    {
        RaiseEvent((byte)code, content, toMasterOption, SendOptions.SendReliable);
    }
    public static void SendUnicastEvent(VrMafiaEventCode code, int target, object content = null)
    {
        SendMulticastEvent(code, new int[] { target }, content);
    }
    public static void SendMulticastEvent(VrMafiaEventCode code, int[] targets, object content = null)
    {
        multicastOption.TargetActors = targets;
        RaiseEvent((byte)code, content, multicastOption, SendOptions.SendReliable);
    }
    public static void SendBroadcastEvent(VrMafiaEventCode code, object content = null)
    {
        RaiseEvent((byte)code, content, broadcastOption, SendOptions.SendReliable);
    }
    #endregion

    #region intro

    #region method
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

    #region event handler
    public override void OnConnectedToMaster()
    {
        wait = false;
        Debug.Log("[Photon Manager] : connected");
        if (PhotonNetwork.NickName.Trim() == "")
            PhotonNetwork.NickName = PhotonNetwork.LocalPlayer.UserId.Substring(0, 6);
        PhotonNetwork.LoadLevel("Lobby");
    }
    public override void OnDisconnected(DisconnectCause cause)
    {
        Connect(PhotonNetwork.NickName);
    }
    #endregion

    #endregion

    #region lobby

    #region callback
    public event Action<List<RoomInfo>> RoomListChanged;
    #endregion

    #region method
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
        if (roomName == "")
            roomName = PhotonNetwork.LocalPlayer.NickName + "'s room";
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

    #region event handler
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
        host = PhotonNetwork.MasterClient;
        PhotonNetwork.AutomaticallySyncScene = true;
        if (PhotonNetwork.IsMasterClient)
            PhotonNetwork.LoadLevel("WaitingRoom");
        Debug.Log("[Photon Manager] : joined room");
        wait = false;
    }
    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        wait = false;
        Debug.Log("[Photon Manager] : join failed");
    }
    #endregion

    #endregion

    #region waiting room

    #region callback
    public event Action PlayerListChanged;
    #endregion

    #region method

    public void LeaveRoom() => PhotonNetwork.LeaveRoom();
    public void GameStart()
    {
        if (!PhotonNetwork.LocalPlayer.IsMasterClient)
            return;
        cr.IsOpen = false;
        foreach (var p in PhotonNetwork.PlayerList)
            p.SetCustomProperties(new Hashtable() { { "Alive", true } });
        PhotonNetwork.LoadLevel("InGame");
    }
    #endregion

    #region event handler
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
        else if (!cr.IsOpen)
        {
            //게임중 퇴장
        }
        else
            PlayerListChanged?.Invoke();

    }
    #endregion

    #endregion
}
