using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class PhotonManager : MonoBehaviourPunCallbacks
{
    #region singleton
    private static PhotonManager instance;
    public static PhotonManager Instance
    {
        get
        {
            if (instance == null)
                instance = GameObject.FindObjectOfType<PhotonManager>();
            return instance;
        }
    }
    #endregion
    [HideInInspector] public Action OnJoined;
    [HideInInspector] public Action OnLeft;
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
    public void Connect(int userId)
    {
        // we check if we are connected or not, we join if we are , else we initiate the connection to the server.
        if (!PhotonNetwork.IsConnected)
        {
            // #Critical, we must first and foremost connect to Photon Online Server.
            PhotonNetwork.GameVersion = "1.0";
            PhotonNetwork.NickName = userId.ToString();
            PhotonNetwork.ConnectUsingSettings();
        }
    }
    public void JoinRoom(int roomId)
    {
        try
        {
            PhotonNetwork.JoinOrCreateRoom(roomId.ToString(), null, null);
        }
        catch { }
    }

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("[Photon Manager] : Join room success");
        OnJoined?.Invoke();
    }
    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.Log("[Photon Manager] : Join Room fail");
        Application.Quit();
    }
    public override void OnLeftRoom()
    {
        Debug.Log("[Photon Manager] : Leave Room");
        OnLeft?.Invoke();
    }
}
