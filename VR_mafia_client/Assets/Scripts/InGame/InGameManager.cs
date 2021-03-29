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

            if(u.Id == TestClientManager.instance.playerID)
            {
                p.AddComponent<Player>();
            }
            else
            {
                p.transform.Find("Head").Find("Main Camera").gameObject.SetActive(false);
            }
        }
    }
}
