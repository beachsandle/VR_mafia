using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using VRKeyboard.Utils;

public class LobbyUIManager : MonoBehaviour
{
    #region field

    #region reference
    private RoomList roomList;
    private Text playerName;
    //change name panel
    private GameObject changeNamePanel;
    private InputField nameInputField;
    //create room panel
    private GameObject createRoomPanel;
    private InputField roomNameInputField;
    #endregion
    public bool isVR;
    private RoomInfo target = null;

    [SerializeField] private KeyboardManager keyboard;
    #endregion

    #region property
    private PhotonManager pm => PhotonManager.Instance;
    #endregion

    #region unity message
    private void Awake()
    {
        if (!pm)
        {
            SceneManager.LoadScene(isVR ? "IntroVR" : "Intro");
            return;
        }
        FindReference();
        Init();
    }
    private void OnDestroy()
    {
        if (pm)
            pm.RoomListChanged -= OnRoomListChanged;
    }
    #endregion

    #region method
    private void FindReference()
    {
        roomList = transform.GetComponentInChildren<RoomList>();
        playerName = transform.Find("PlayerName").GetComponent<Text>();

        changeNamePanel = transform.Find("ChangeName Panel").gameObject;
        nameInputField = changeNamePanel.transform.GetComponentInChildren<InputField>();

        createRoomPanel = transform.Find("CreateRoom Panel").gameObject;
        roomNameInputField = createRoomPanel.transform.GetComponentInChildren<InputField>();
    }
    private void Init()
    {
        roomList.RoomClicked += OnRoomClicked;
        roomList.RoomDoubleClicked += OnJoinButton;
        playerName.text = PhotonNetwork.NickName;
        pm.RoomListChanged += OnRoomListChanged;
        pm.JoinLobby();

        if (keyboard)
        {
            InitKeyboard();
        }
    }
    private void InitKeyboard()
    {
        Transform row;

        row = keyboard.keys.Find("row0");
        row.Find("Back").GetComponent<Button>().onClick.AddListener(keyboard.Backspace);

        row = keyboard.keys.Find("row2");
        row.Find("CapsLock").GetComponent<Button>().onClick.AddListener(keyboard.CapsLock);

        row = keyboard.keys.Find("row3");
        row.Find("Shift").GetComponent<Button>().onClick.AddListener(keyboard.Shift);

        row = keyboard.keys.Find("row4");
        row.Find("Clear").GetComponent<Button>().onClick.AddListener(keyboard.Clear);
        row.Find("Enter").GetComponent<Button>().onClick.AddListener(() => OnEnterKey(keyboard.inputText));
    }
    #endregion

    #region event handler
    private void OnRoomListChanged(List<RoomInfo> roomInfos)
    {
        roomList.Clear();
        roomList.CreateRooms(roomInfos);
    }
    private void OnRoomClicked(RoomInfo roomInfo) => target = roomInfo;

    #region button event
    public void OnJoinButton()
    {
        Debug.Log($"lobby : {target}");
        if (target != null)
            pm.JoinRoom(target);
    }
    public void OnRefreshButton() => pm.RefreshRoomList();

    #region ChangeNamePanel
    public void OnChangeNameButton()
    {
        nameInputField.text = playerName.text;
        changeNamePanel.SetActive(true);

        if (keyboard) keyboard.gameObject.SetActive(true);
    }
    public void OnChangeNameOKButton()
    {
        if (nameInputField.text != "")
        {
            PhotonNetwork.NickName = nameInputField.text;
            playerName.text = nameInputField.text;
        }
        changeNamePanel.SetActive(false);
    }
    public void OnChangeNameNOButton()
    {
        changeNamePanel.SetActive(false);
    }
    #endregion

    #region CreateRoomPanel
    public void OnCreateRoomButton()
    {
        roomNameInputField.text = "";
        createRoomPanel.SetActive(true);

        if (keyboard) keyboard.gameObject.SetActive(true);
    }
    public void OnCreateRoomOKButton()
    {
        pm.CreateRoom(roomNameInputField.text);
        createRoomPanel.SetActive(false);
    }
    public void OnCreateRoomNOButton()
    {
        createRoomPanel.SetActive(false);
    }
    #endregion

    public void OnEnterKey(Text text)
    {
        if (changeNamePanel.activeSelf) nameInputField.text = text.text;
        if (createRoomPanel.activeSelf) roomNameInputField.text = text.text;

        keyboard.gameObject.SetActive(false);
    }
    #endregion 

    #endregion
}
