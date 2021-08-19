using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LobbyManager : MonoBehaviour
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

    private RoomInfo target = null;
    #endregion

    #region property
    private PhotonManager pm => PhotonManager.Instance;
    #endregion

    #region unity message
    private void Awake()
    {
        if (pm == null)
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
        pm.LeftLobby += OnLeftLobby;
        pm.RoomListChanged += OnRoomListChanged;
    }
    #endregion

    #region event handler
    private void OnLeftLobby()
    {
        pm.LeftLobby -= OnLeftLobby;
        pm.RoomListChanged -= OnRoomListChanged;
    }
    private void OnRoomListChanged(List<RoomInfo> roomInfos)
    {
        roomList.Clear();
        roomList.CreateRoom(roomInfos);
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
    }
    public void OnCreateRoomOKButton()
    {
        if (roomNameInputField.text != "")
            pm.CreateRoom(roomNameInputField.text);
        createRoomPanel.SetActive(false);
    }
    public void OnCreateRoomNOButton()
    {
        createRoomPanel.SetActive(false);
    }
    #endregion

    #endregion 

    #endregion
}
