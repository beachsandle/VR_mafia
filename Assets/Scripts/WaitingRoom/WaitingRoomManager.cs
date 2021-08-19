using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WaitingRoomManager : MonoBehaviour
{
    #region field

    private Text roomName;

    #endregion

    #region property
    private PhotonManager pm => PhotonManager.Instance;
    #endregion

    #region unity message
    private void Awake()
    {
        if (pm == null || PhotonNetwork.CurrentRoom == null)
        {
            SceneLoader.ReturnToIntroScene();
            return;
        }
        FindReference();
        Init();
    }
    #endregion

    #region method
    private void FindReference()
    {
        roomName = transform.Find("RoomName").GetComponent<Text>();
    }
    private void Init()
    {
        roomName.text = PhotonNetwork.CurrentRoom.Name;
    }
    #endregion

    #region event handler

    #region button event
    public void OnLeaveButton()
    {
    }
    public void OnStartButton()
    {

    }

    #endregion 

    #endregion
}
