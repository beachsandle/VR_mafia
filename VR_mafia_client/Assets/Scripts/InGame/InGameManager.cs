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

    private bool isMafia;

    [Header("UI")]
    [SerializeField]
    private Text roleText;
    [SerializeField]
    private Text informationText;
    private float fadeTime = 3f;

    [Header("Menu Panel")]
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private Button backButton;
    [HideInInspector] public bool menuState = false;

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
        isMafia = TestClientManager.instance.isMafia;

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
            p.transform.Find("Head").GetComponent<MeshRenderer>().material.color = Global.colors[idx]; // 임시
            p.transform.Find("Body").GetComponent<MeshRenderer>().material.color = Global.colors[idx]; // 임시
            p.transform.position = spawnPos.GetChild(idx++).position;

            if(u.Id == TestClientManager.instance.playerID)
            {
                p.AddComponent<Player>();

                roleText.text = isMafia ? "마피아" : "시민";
                StartInformation(string.Format("당신은 {0}입니다", roleText.text));
                
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

    public void StartInformation(string s)
    {
        informationText.gameObject.SetActive(true);
        informationText.text = s;

        StartCoroutine(FadeOutInformationText());
    }
    private IEnumerator FadeOutInformationText()
    {
        Color orginColor = new Color(informationText.color.r, informationText.color.g, informationText.color.b, 1);
        Color clearColor = new Color(informationText.color.r, informationText.color.g, informationText.color.b, 0);
        float time = 0f;

        while(informationText.color.a > 0.0f)
        {
            informationText.color = Color.Lerp(orginColor, clearColor, time / fadeTime);
            time += Time.deltaTime;

            yield return null;
        }
        informationText.color = clearColor;

        informationText.text = "";
        informationText.gameObject.SetActive(false);
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
