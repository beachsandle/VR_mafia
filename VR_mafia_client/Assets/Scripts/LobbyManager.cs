using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LobbyManager : MonoBehaviour
{
    [SerializeField] private GameObject RoomList;

    [Header("Button")]
    [SerializeField] private Button JoinButton;
    [SerializeField] private Button CreateButton;
    [SerializeField] private Button RefreshButton;

    void Start()
    {
        CreateButton.onClick.AddListener(OnCreateButton);
    }

    void OnCreateButton()
    {

    }
}
