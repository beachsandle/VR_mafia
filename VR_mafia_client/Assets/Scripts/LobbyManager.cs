using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LobbyManager : MonoBehaviour
{
    [SerializeField] private GameObject roomList;

    [Header("Button")]
    [SerializeField] private Button joinButton;
    [SerializeField] private Button createButton;
    [SerializeField] private Button refreshButton;

    void Start()
    {
        createButton.onClick.AddListener(OnCreateButton);
    }

    void OnJoinButton()
    {

    }

    void OnCreateButton()
    {
        roomList.GetComponent<RoomList>().CreateRoom();
    }

    void OnRefreshButton()
    {

    }
}
