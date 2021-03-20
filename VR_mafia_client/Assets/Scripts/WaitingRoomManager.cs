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
    static Color[] colors = { Color.red, Color.green, Color.blue, Color.cyan, Color.magenta, Color.yellow, Color.gray, Color.black };
    const int HEAD_COUNT = 8;

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

        for (int i = 0; i < TestClientManager.instance.users.Count; i++)
        {
            SetPlayerInfo(i);
        }
    }

    public void AddPlayer(UserInfo user)
    {
        //TODO: SetPlayerInfo()로 교체
        //SetPlayerInfo(TestClientManager.instance.users.Count);

        playerTRs[TestClientManager.instance.users.Count].Find("Name").GetComponent<Text>().text = user.Name;
        playerTRs[TestClientManager.instance.users.Count].Find("Image").GetComponent<Image>().color = colors[TestClientManager.instance.users.Count];
    }

    private void SetPlayerInfo(int playerNum)
    {
        playerTRs[playerNum].Find("Name").GetComponent<Text>().text = TestClientManager.instance.users[playerNum].Name;
        playerTRs[playerNum].Find("Image").GetComponent<Image>().color = colors[playerNum];
    }

    private void SetRoomName(string roomName)
    {
        roomNameText.text = roomName;
    }

    void OnStartButton()
    {
        TestClientManager.instance.EmitGameStartReq();
    }
    void OnLeaveButton()
    {
        //TODO: 로비로 돌아가는 코드 추가
    }
}
