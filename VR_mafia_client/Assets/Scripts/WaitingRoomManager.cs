using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MyPacket;

public class WaitingRoomManager : MonoBehaviour
{
    [Header("Button")]
    [SerializeField] private Button startButton;

    public Text[] p;

    int idNum = 1;

    public static WaitingRoomManager instance;
    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        startButton.onClick.AddListener(OnStartButton);

        for(int i = 0; i < TestClientManager.instance.users.Count; i++)
        {
            p[i].text = TestClientManager.instance.users[i].Name;
        }
    }

    public void AddPlayer(UserInfo user)
    {
        var p = GameObject.Find("PlayerT" + idNum++);
        p.GetComponent<Text>().text = user.Name;
    }

    void OnStartButton()
    {
        TestClientManager.instance.EmitGameStartReq();
    }
}
