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

    public Dictionary<int, GameObject> players;


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
        players = new Dictionary<int, GameObject>();

        SpawnPlayers();
    }

    void Update()
    {
        
    }

    private void SpawnPlayers()
    {
        foreach(UserInfo u in TestClientManager.instance.users)
        {
            GameObject p = Instantiate(playerObj);
            p.name = "Player_" + u.Id;
            p.transform.position += new Vector3(interval++, 0, 0);

            if(u.Id == TestClientManager.instance.playerID)
            {
                p.AddComponent<Player>();

                Camera.main.transform.parent = p.transform.Find("Head");
                Camera.main.transform.localPosition = new Vector3(0, 0, 0);
            }

            players.Add(u.Id, p);
        }
    }

    public void UpdatePlayerTransform(MoveData data)
    {
        V3 pos = data.location.position;
        V3 rot = data.location.rotation;

        Transform TR = players[data.player_id].transform;
        TR.position = new Vector3(pos.x, pos.y, pos.z);
        TR.rotation = Quaternion.Euler(rot.x, rot.y, rot.z);
    }
}
