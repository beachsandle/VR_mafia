using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyPacket;

public class InGameManager : MonoBehaviour
{
    private static InGameManager _instance;
    public static InGameManager instance
    {
        get
        {
            if (!_instance)
            {
                _instance = FindObjectOfType(typeof(InGameManager)) as InGameManager;
            }

            return _instance;
        }
    }

    [SerializeField]
    private GameObject playerObj;
    private int interval = 0;

    public List<UserInfo> players;


    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        players = TestClientManager.instance.users;

        SpawnPlayers();
    }

    void Update()
    {
        
    }

    void SpawnPlayers()
    {
        foreach(UserInfo u in players)
        {
            GameObject p = Instantiate(playerObj);
            p.name = "Player_" + u.Id;
            p.transform.position += new Vector3(interval++, 0, 0);

            if(u.Id == TestClientManager.instance.playerID)
            {
                p.AddComponent<Player>();
            }
            else
            {
                // TODO: 카메라 추가하는 방식 변경
                p.transform.Find("Head").Find("Main Camera").gameObject.SetActive(false);
            }
        }
    }
}
