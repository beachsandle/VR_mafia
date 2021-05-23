using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LobbyManager : MonoBehaviour
{
    private static LobbyManager _instance = null;
    public static LobbyManager instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<LobbyManager>();
            }
            return _instance;
        }
    }

    public RoomList roomList;

    [Header("UI")]
    [SerializeField] private Text playerName;

    [Header("Button")]
    [SerializeField] private Button joinButton;
    [SerializeField] private Button createButton;
    [SerializeField] private Button refreshButton;
    [SerializeField] private Button changeNameButton;

    [Header("ChangeName Panel")]
    [SerializeField] private GameObject changeNamePanel;
    [SerializeField] private InputField nameInputField;

    [Header("CreateRoom Panel")]
    [SerializeField] private GameObject createRoomPanel;
    [SerializeField] private InputField roomNameInputField;

    void Start()
    {
        createButton.onClick.AddListener(OnCreateRoomButton);
        joinButton.onClick.AddListener(OnJoinButton);
        refreshButton.onClick.AddListener(OnRefreshButton);
        changeNameButton.onClick.AddListener(OnChangeNameButton);

        InitChangeNamePanel();
        InitCreateRoomPanel();

        playerName.text = ClientManager.instance.userName;
        ClientManager.instance.EmitSetNameReq(playerName.text);
        ClientManager.instance.EmitRoomListReq();
    }
    public void UpdateRooms(string rn, string hn, int p, int i) // 임시
    {
        roomList.CreateRoom(rn, hn, p, i);
    }

    void OnJoinButton()
    {
        ClientManager.instance.EmitJoinRoomReq(roomList.select);
    }

    void OnRefreshButton()
    {
        roomList.Clear();
        ClientManager.instance.EmitRoomListReq();
    }

    #region CreateRoomPanel
    void InitCreateRoomPanel()
    {
        createRoomPanel.SetActive(true);

        createRoomPanel.transform.GetChild(0).Find("OK Button").GetComponent<Button>().onClick.AddListener(OnCreateRoomOKButton);
        createRoomPanel.transform.GetChild(0).Find("NO Button").GetComponent<Button>().onClick.AddListener(OnCreateRoomNOButton);
        nameInputField.characterLimit = 10;

        createRoomPanel.SetActive(false);
    }
    void OnCreateRoomButton()
    {
        createRoomPanel.SetActive(true);
        roomNameInputField.text = "";
    }
    void OnCreateRoomOKButton()
    {
        ClientManager.instance.EmitCreateRoomReq(roomNameInputField.text);
        createRoomPanel.SetActive(false);
    }
    void OnCreateRoomNOButton()
    {
        createRoomPanel.SetActive(false);
    }
    #endregion

    #region ChangeNamePanel
    void InitChangeNamePanel()
    {
        changeNamePanel.SetActive(true);

        changeNamePanel.transform.GetChild(0).Find("OK Button").GetComponent<Button>().onClick.AddListener(OnChangeNameOKButton);
        changeNamePanel.transform.GetChild(0).Find("NO Button").GetComponent<Button>().onClick.AddListener(OnChangeNameNOButton);
        nameInputField.characterLimit = 10;

        changeNamePanel.SetActive(false);
    }
    void OnChangeNameButton()
    {
        changeNamePanel.SetActive(true);
        nameInputField.text = playerName.text;
    }
    void OnChangeNameOKButton()
    {
        ClientManager.instance.userName = nameInputField.text;
        ClientManager.instance.EmitSetNameReq(nameInputField.text);
        playerName.text = nameInputField.text;
        changeNamePanel.SetActive(false);
    }
    void OnChangeNameNOButton()
    {
        changeNamePanel.SetActive(false);
    }
    #endregion
}
