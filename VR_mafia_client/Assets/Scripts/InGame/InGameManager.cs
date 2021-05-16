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
    public bool phaseChange;
    private bool suffrage;

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

    [Header("Voting Panel")]
    [SerializeField] private GameObject votingPanel;
    [SerializeField] private Text timeText;

    public Dictionary<int, GameObject> players; // id, object
    private GameObject playerObjects; // 임시

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
        playerObjects = GameObject.Find("PlayerObjects");

        players = new Dictionary<int, GameObject>();
        isMafia = ClientManager.instance.isMafia;

        SpawnPlayers();

        InitMenuPanel();
        InitVotingPanel();

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

        foreach (UserInfo u in ClientManager.instance.users)
        {
            GameObject p = Instantiate(playerObj);
            p.name = "Player_" + u.Id;
            p.transform.Find("Head").GetComponent<MeshRenderer>().material.color = Global.colors[idx]; // 임시
            p.transform.Find("Body").GetComponent<MeshRenderer>().material.color = Global.colors[idx]; // 임시
            p.transform.position = spawnPos.GetChild(idx++).position;

            if(u.Id == ClientManager.instance.playerID)
            {
                p.AddComponent<Player>();

                roleText.text = isMafia ? "마피아" : "시민";
                StartInformation(string.Format("당신은 {0}입니다", roleText.text));
                
                Camera.main.transform.parent = p.transform.Find("Head");
                Camera.main.transform.localPosition = new Vector3(0, 0, 0);
            }

            p.transform.parent = GameObject.Find("PlayerObjects").transform;
            players.Add(u.Id, p);
        }
    }

    private void GatherPlayers()
    {
        Transform spawnPos = GameObject.Find("SpawnPosition").transform;
        int idx = 0;

        foreach(GameObject p in players.Values)
        {
            p.GetComponent<Player>().CC.enabled = false;
            p.transform.position = spawnPos.GetChild(idx++).position;
            p.GetComponent<Player>().CC.enabled = true;
        }
    }

    #region Phase
    public void StartDay()
    {
        StartInformation("낮이 되었습니다.");
    }

    public void StartNight()
    {
        phaseChange = true;

        StartInformation("밤이 되었습니다.");
        GatherPlayers();

        phaseChange = false;
    }

    public void StartPhaseChange()
    {
        StartCoroutine(PhaseChange());
    }
    private IEnumerator PhaseChange()
    {
        phaseChange = true;

        float time = 1f;
        while(0f < time)
        {
            time -= Time.deltaTime;

            yield return null;
        }

        GatherPlayers();
        phaseChange = false;
    }
    #endregion

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

        informationText.text = "";
        informationText.color = orginColor;
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

    #region VotingPanel
    private void InitVotingPanel()
    {
        votingPanel.SetActive(true);

        var childs = votingPanel.transform.GetChild(0);
        for (int i = 0; i < 10; i++)
        {
            int pNum = i + 1;

            Transform buttonTR = childs.GetChild(pNum);
            if (pNum <= players.Count)
            {
                buttonTR.Find("Image").GetComponent<Image>().color = Global.colors[i];
                buttonTR.GetComponent<Button>().onClick.AddListener(() => { OnVoteButton(pNum); }); // local 변수 써야함 건들지 말 것
            }
            else
            {
                Destroy(buttonTR.gameObject);
            }
        }

        votingPanel.SetActive(false);
    }
    private void OnVoteButton(int pNum)
    {
        if (suffrage)
        {
            suffrage = false;

            string s = playerObjects.transform.GetChild(pNum - 1).name;
            ClientManager.instance.EmitVoteReq(int.Parse(s.Replace("Player_", "")));
        }
    }

    public void OnVotingPanel(int time)
    {
        suffrage = true;

        votingPanel.SetActive(true);

        StartCoroutine(UpdateVotingTime(time));
    }
    IEnumerator UpdateVotingTime(int time)
    {
        while (0 < time)
        {
            timeText.text = time.ToString();

            time--;
            yield return new WaitForSeconds(1f);
        }

        OffVotingPanel();
    }
    private void OffVotingPanel()
    {
        votingPanel.SetActive(false);
    }
    #endregion
}
