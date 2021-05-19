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
    public bool suffrage;

    [Header("UI")]
    [SerializeField] private Text roleText;
    [SerializeField] private Text informationText;
    private float fadeTime = 3f;

    [Header("Menu Panel")]
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private Button backButton;
    [HideInInspector] public bool menuState = false;

    [Header("Voting Panel")]
    [SerializeField] private GameObject votingPanel;
    private Text timeText;

    [Header("Final Voting Panel")]
    [SerializeField] private GameObject finalVotingPanel;
    private Text finalTimeText;

    public Dictionary<int, GameObject> players; // id, object
    public List<int> playerOrder;
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
        playerOrder = new List<int>();
        isMafia = ClientManager.instance.isMafia;

        SpawnPlayers();

        InitMenuPanel();
        InitVotingPanel();
        InitFinalVotingPanel();

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
            playerOrder.Add(u.Id);
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

        for(int i = 0; i < playerOrder.Count; i++)
        {
            if (playerOrder[i] == ClientManager.instance.playerID)
            {
                GameObject myObject = players[playerOrder[i]];
                myObject.GetComponent<Player>().CC.enabled = false;
                myObject.transform.position = spawnPos.GetChild(i).position;
                myObject.GetComponent<Player>().CC.enabled = true;

                ClientManager.instance.EmitMove(myObject.transform.position, myObject.transform.rotation);
            }
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

    public void UpdatePlayerTransform(MoveEventData eventData)
    {
        foreach(var data in eventData.movedPlayer)
        {
            if (data.player_id == ClientManager.instance.playerID) continue;

            V3 pos = data.location.position;
            V3 rot = data.location.rotation;

            Transform TR = players[data.player_id].transform;
            TR.position = new Vector3(pos.x, pos.y, pos.z);
            TR.rotation = Quaternion.Euler(rot.x, rot.y, rot.z);
        }
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

        var votingContent = votingPanel.transform.GetChild(0);
        for (int i = 0; i < 10; i++)
        {
            int pNum = i + 1;

            Transform buttonTR = votingContent.GetChild(pNum);
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
        timeText = votingContent.Find("Time Text").GetComponent<Text>();

        votingPanel.SetActive(false);
    }
    private void OnVoteButton(int pNum)
    {
        if (suffrage)
        {
            string s = playerObjects.transform.GetChild(pNum - 1).name;
            ClientManager.instance.EmitVoteReq(int.Parse(s.Replace("Player_", "")));

            suffrage = false; // TODO: OnVoteRes() 에서 처리하도록 변경
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

    public void DisplayVotingResult((int pid, int count)[] result)
    {
        var votingContent = votingPanel.transform.GetChild(0);
        for(int i = 0; i < players.Count; i++)
        {
            votingContent.GetChild(i + 1).Find("Count Text").GetComponent<Text>().text = "";
        }

        for(int i = 0; i < result.Length; i++)
        {
            int id = result[i].pid;

            for(int j = 0; j < result.Length; j++)
            {
                if(id == playerOrder[j])
                {
                    Text countText = votingContent.GetChild(j + 1).Find("Count Text").GetComponent<Text>();
                    countText.text = result[i].count.ToString();

                    break;
                }
            }
        }
    }
    #endregion

    #region FinalVotingPanel
    private void InitFinalVotingPanel()
    {
        finalVotingPanel.SetActive(true);

        var finalVotingContent = finalVotingPanel.transform.GetChild(0);
        
        Button btn;
        btn = finalVotingContent.Find("Pros Button").GetComponent<Button>();
        btn.onClick.AddListener(() => { OnProsConsButton(true); });
        btn.gameObject.SetActive(false);

        btn = finalVotingContent.Find("Cons Button").GetComponent<Button>();
        btn.onClick.AddListener(() => { OnProsConsButton(false); });
        btn.gameObject.SetActive(false);

        finalTimeText = finalVotingContent.Find("Time Text").GetComponent<Text>();

        finalVotingPanel.SetActive(false);
    }
    private void OnProsConsButton(bool isPros)
    {
        if (suffrage)
        {
            ClientManager.instance.EmitFinalVoteReq(isPros);

            suffrage = false; // TODO: OnFinalVoteRes() 에서 처리하도록 변경
        }
    }

    public void OnFinalVotingPanel()
    {
        finalVotingPanel.SetActive(true);
    }
    public void StartDefense(int defenseTime)
    {
        StartCoroutine(UpdateFinalVotingTime(defenseTime, false));
    }
    public void StartFinalVoting(int time)
    {
        suffrage = true;

        var finalVotingContent = finalVotingPanel.transform.GetChild(0);
        finalVotingContent.Find("Pros Button").gameObject.SetActive(true);
        finalVotingContent.Find("Cons Button").gameObject.SetActive(true);

        StartCoroutine(UpdateFinalVotingTime(time, true));
    }
    IEnumerator UpdateFinalVotingTime(int time, bool isFinal)
    {
        while (0 < time)
        {
            finalTimeText.text = time.ToString();

            time--;
            yield return new WaitForSeconds(1f);
        }

        if (isFinal)
        {
            var finalVotingContent = finalVotingPanel.transform.GetChild(0);
            finalVotingContent.Find("Pros Button").gameObject.SetActive(false);
            finalVotingContent.Find("Cons Button").gameObject.SetActive(false);

            OffFinalVotingPanel();
        }
    }
    private void OffFinalVotingPanel()
    {
        finalVotingPanel.SetActive(false);
    }
    
    public void DisplayFinalVotingResult(int id)
    {
        Debug.Log(id + "Kicked...");
    }
    #endregion
}
