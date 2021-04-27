using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
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

    [Header("Menu Panel")]
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private Button backButton;
    [HideInInspector] public bool menuState = false;

    static Color[] colors = { Color.red, Color.green, Color.blue, Color.cyan, Color.magenta, Color.yellow, Color.gray, Color.black }; // 임시

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

        InitMenuPanel();
        SpawnPlayers();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            menuState = !menuState;
            SetActiveMenuPanel(menuState);
        }
    }

    private void SpawnPlayers()
    {
        Transform spawnPos = GameObject.Find("SpawnPosition").transform;
        int idx = 0;

        foreach (UserInfo u in TestClientManager.instance.users)
        {
            GameObject p = Instantiate(playerObj);
            p.name = "Player_" + u.Id;
            p.transform.Find("Head").GetComponent<MeshRenderer>().material.color = colors[idx]; // 임시
            p.transform.Find("Body").GetComponent<MeshRenderer>().material.color = colors[idx]; // 임시
            p.transform.position = spawnPos.GetChild(idx++).position;

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

    #region MenuPanel
    private void InitMenuPanel()
    {
        menuPanel.SetActive(true);

        menuPanel.transform.GetChild(0).Find("Back Button").GetComponent<Button>().onClick.AddListener(OnBackButton);

        menuPanel.SetActive(false);
    }
    private void SetActiveMenuPanel(bool state)
    {
        if (state)
        {
            menuPanel.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            menuPanel.SetActive(false);
        }
    }
    private void OnBackButton()
    {
        menuState = false;
        SetActiveMenuPanel(menuState);
    }
    #endregion
}
