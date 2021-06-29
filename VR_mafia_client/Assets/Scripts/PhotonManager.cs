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

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
    private void Start() {
        Connect();        
    }
    public void Connect()
        {
            // we check if we are connected or not, we join if we are , else we initiate the connection to the server.
            if (!PhotonNetwork.IsConnected)
            {
                // #Critical, we must first and foremost connect to Photon Online Server.
                PhotonNetwork.GameVersion = "1.0";
                PhotonNetwork.ConnectUsingSettings();
            }
        }
    public void JoinRoom(int roomId)
    {
        PhotonNetwork.JoinOrCreateRoom(roomId.ToString(), null, null);
    }

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Join room success");
    }
    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.Log("Join Room fail");
        Application.Quit();
    }
    public override void OnLeftRoom()
    {
        Debug.Log("Leave Room");
    }

}
