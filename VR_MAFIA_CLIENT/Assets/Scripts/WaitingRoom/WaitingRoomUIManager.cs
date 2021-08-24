﻿using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class WaitingRoomUIManager : MonoBehaviour
{
    #region field

    private Text roomName;
    private Transform players;

    #endregion

    #region property
    private PhotonManager pm => PhotonManager.Instance;
    #endregion

    #region unity message
    private void Awake()
    {
        if (!pm || PhotonNetwork.CurrentRoom == null)
        {
            SceneManager.LoadScene("Intro");
            return;
        }
        FindReference();
        Init();
    }
    private void OnDestroy()
    {
        if (pm)
            pm.PlayerListChanged -= SetPlayers;
    }
    #endregion

    #region method
    private void FindReference()
    {
        roomName = transform.Find("RoomName").GetComponent<Text>();
        players = transform.Find("Players");
    }
    private void Init()
    {
        roomName.text = PhotonNetwork.CurrentRoom.Name;
        SetPlayers();
        pm.PlayerListChanged += SetPlayers;
    }
    private void SetPlayers()
    {
        for (int i = 0; i < 10; ++i)
        {
            var p = players.GetChild(i);
            var img = p.GetComponentInChildren<Image>();
            var name = p.GetComponentInChildren<Text>();
            if (pm.PlayerList.Length > i)
            {
                img.color = Global.colors[i];
                name.text = pm.PlayerList[i].NickName;
            }
            else
            {
                img.color = Color.white;
                name.text = "";
            }
        }
    }
    #endregion

    #region event handler
    #region button event
    public void OnLeaveButton() => pm.LeaveRoom();
    public void OnStartButton() => pm.GameStart();

    #endregion 

    #endregion
}
