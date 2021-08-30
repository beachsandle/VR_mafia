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
    public bool isVR;

    private bool wait = false;
    private Player host;
    private static readonly TypedLobby defaultLobby = new TypedLobby(null, LobbyType.SqlLobby);
    private static readonly RaiseEventOptions toMasterOption = new RaiseEventOptions() { Receivers = ReceiverGroup.MasterClient };
    private static readonly RaiseEventOptions multicastOption = new RaiseEventOptions() { Receivers = ReceiverGroup.Others };
    private static readonly RaiseEventOptions broadcastOption = new RaiseEventOptions() { Receivers = ReceiverGroup.All };
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
    public static void UnicastEvent(VrMafiaEventCode code, int target, object content = null)
    {
        MulticastEvent(code, new int[] { target }, content);
    }
    public static void MulticastEvent(VrMafiaEventCode code, int[] targets, object content = null)
    {
        multicastOption.TargetActors = targets;
        RaiseEvent((byte)code, content, multicastOption, SendOptions.SendReliable);
    }
    public static void BroadcastEvent(VrMafiaEventCode code, object content = null)
    {
        RaiseEvent((byte)code, content, broadcastOption, SendOptions.SendReliable);
    }
    public static void SetVoiceName(string name)
    {
        LocalPlayer.SetCustomProperties(new Hashtable() { { "VoiceName", name } });
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
            GameVersion = "1.0";
            NickName = nickname;
            ConnectUsingSettings();
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
        if (NickName.Trim() == "")
            NickName = LocalPlayer.UserId.Substring(0, 6);
        SceneLoader.Instance.Load(isVR ? "LobbyVR" : "Lobby");
    }
    public override void OnDisconnected(DisconnectCause cause)
    {
        Connect(NickName);
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
            roomName = LocalPlayer.NickName + "'s room";
        var option = new RoomOptions();
        option.MaxPlayers = 10;
        option.CustomRoomProperties = new Hashtable() { { "HostName", NickName } };
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
        CurrentRoom.SetMasterClient(LocalPlayer);
    }
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        wait = false;
        Debug.Log("[Photon Manager] : create failed");
    }
    public override void OnJoinedRoom()
    {
        host = MasterClient;
        SceneLoader.Instance.Load(isVR ? "WaitingRoomVR" : "WaitingRoom");
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
        if (!LocalPlayer.IsMasterClient)
            return;
        CurrentRoom.IsOpen = false;
        CurrentRoom.SetCustomProperties(new Hashtable() { { "Started", true } });
        foreach (var p in PlayerList)
            p.SetCustomProperties(new Hashtable() { { "Alive", true } });
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
        else if (CurrentRoom.IsOpen)
            PlayerListChanged?.Invoke();

    }
    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        if (propertiesThatChanged.ContainsKey("Started") && (bool)propertiesThatChanged["Started"])
        {
            IsMessageQueueRunning = false;
            SceneLoader.Instance.Load(isVR ? "InGameVR" : "InGame");
        }
    }
    #endregion

    #endregion
}
