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
            {
                instance = GameObject.FindObjectOfType<PhotonManager>();
                DontDestroyOnLoad(instance.gameObject);
            }
            return instance;
        }
    }
    #endregion

    public void JoinRoom(int roomId)
    {
        PhotonNetwork.JoinOrCreateRoom(roomId.ToString(), null, null);
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

}
