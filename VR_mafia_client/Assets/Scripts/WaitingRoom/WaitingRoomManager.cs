using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MyPacket;

public class WaitingRoomManager : MonoBehaviour
{
    [Header("Button")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button leaveButton;

    [Header("UI")]
    [SerializeField] private Text roomNameText;

    private Transform[] playerTRs;
    const int HEAD_COUNT = 10;

    public static WaitingRoomManager instance;
    void Awake()
    {
        instance = this;

        playerTRs = new Transform[HEAD_COUNT];
        for (int i = 0; i < HEAD_COUNT; i++)
        {
            playerTRs[i] = GameObject.Find("Player" + (i + 1)).transform;
        }
    }

    void Start()
    {
        startButton.onClick.AddListener(OnStartButton);
        leaveButton.onClick.AddListener(OnLeaveButton);

        for (int i = 0; i < ClientManager.instance.users.Count; i++)
        {
            SetPlayerInfo(i);
        }
    }

    public void AddPlayer(UserInfo user)
    {
        //TODO: SetPlayerInfo()로 교체
        //SetPlayerInfo(ClientManager.instance.users.Count);

        playerTRs[ClientManager.instance.users.Count - 1].Find("Name").GetComponent<Text>().text = user.Name;
        playerTRs[ClientManager.instance.users.Count - 1].Find("Image").GetComponent<Image>().color = Global.colors[ClientManager.instance.users.Count - 1];
    }

    public void RemovePlayer(int playerId)
    {
        for (int i = 0; i < ClientManager.instance.users.Count; i++)
        {
            SetPlayerInfo(i);
        }
        for(int i = ClientManager.instance.users.Count; i < HEAD_COUNT; i++)
        {
            playerTRs[i].Find("Name").GetComponent<Text>().text = "P" + (i + 1);
            playerTRs[i].Find("Image").GetComponent<Image>().color = Color.white;
        }
    }

    private void SetPlayerInfo(int playerNum)
    {
        playerTRs[playerNum].Find("Name").GetComponent<Text>().text = ClientManager.instance.users[playerNum].Name;
        playerTRs[playerNum].Find("Image").GetComponent<Image>().color = Global.colors[playerNum];
    }

    private void SetRoomName(string roomName)
    {
        roomNameText.text = roomName;
    }

    void OnStartButton()
    {
        ClientManager.instance.EmitGameStartReq();
    }
    void OnLeaveButton()
    {
        ClientManager.instance.EmitLeaveRoomReq();
    }
}
