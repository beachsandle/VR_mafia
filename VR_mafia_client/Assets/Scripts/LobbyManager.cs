using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LobbyManager : MonoBehaviour
{
    public static LobbyManager instance;
    void Awake()
    {
        instance = this;
    }

    [SerializeField] private RoomList roomList;
    [Header("UI")]
    [SerializeField] private Text playerName;

    [Header("Button")]
    [SerializeField] private Button joinButton;
    [SerializeField] private Button createButton;
    [SerializeField] private Button refreshButton;
    [SerializeField] private Button changeNameButton;

    [Header("ChangeNamePanel")]
    [SerializeField] private GameObject changeNamePanel;
    [SerializeField] private InputField inputField;
    [SerializeField] private Button okButton;
    [SerializeField] private Button noButton;

    void Start()
    {
        createButton.onClick.AddListener(OnCreateButton);
        joinButton.onClick.AddListener(OnJoinButton);
        refreshButton.onClick.AddListener(OnRefreshButton);

        InitChangeNamePanel();
    }

    void OnJoinButton()
    {
        TestClientManager.instance.EmitJoinRoomReq(1);
    }

    void OnCreateButton()
    {
        // roomList.CreateRoom();
        TestClientManager.instance.EmitCreateRoomReq("new room");
    }

    void OnRefreshButton()
    {
        TestClientManager.instance.EmitRoomListReq();
    }

    public void UpdateRooms(string rn, string hn, int p, int i) // 임시
    {
        roomList.CreateRoom(rn, hn, p, i);
    }

    #region ChangeNamePanel
    void InitChangeNamePanel()
    {
        changeNamePanel.SetActive(true);

        changeNameButton.onClick.AddListener(OnChangeNameButton);
        okButton.onClick.AddListener(OnOKButton);
        noButton.onClick.AddListener(OnNoButton);

        inputField.characterLimit = 10;

        changeNamePanel.SetActive(false);
    }
    void OnChangeNameButton()
    {
        changeNamePanel.SetActive(true);
        inputField.text = playerName.text;
    }
    void OnOKButton()
    {
        playerName.text = inputField.text;
        changeNamePanel.SetActive(false);
    }
    void OnNoButton()
    {
        changeNamePanel.SetActive(false);
    }
    #endregion
}
