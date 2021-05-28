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

    //private Global.GameStatus gameStatus;

    [SerializeField]
    private GameObject playerObj;

    private bool isMafia;
    public bool phaseChange;
    public bool isVoting;
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
    public Player myInfo;

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

        HideCursor();

        FadeInOut.instance.FadeIn();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            menuState = !menuState;
            SetActiveMenuPanel(menuState);
        }
    }

    public void UpdatePlayerTransform(MoveEventData data)
    {
        if (data.Player_id == ClientManager.instance.playerID) return;

        V3 pos = data.Location.position;
        V3 rot = data.Location.rotation;

        Transform TR = players[data.Player_id].transform;
        TR.position = new Vector3(pos.x, pos.y, pos.z);
        TR.rotation = Quaternion.Euler(rot.x, rot.y, rot.z);
    }

    private void ShowCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    private void HideCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void SpawnPlayers()
    {
        Transform spawnPos = GameObject.Find("SpawnPosition").transform;
        int idx = 0;

        foreach (UserInfo u in ClientManager.instance.users)
        {
            playerOrder.Add(u.Id);

            GameObject p = Instantiate(playerObj);
            p.name = "Player_" + u.Id;
            p.transform.Find("Head").GetComponent<MeshRenderer>().material.color = Global.colors[idx];
            p.transform.Find("Body").GetComponent<MeshRenderer>().material.color = Global.colors[idx];
            p.transform.position = spawnPos.GetChild(idx).position;
            p.transform.parent = playerObjects.transform;

            p.GetComponent<Player>().InitPlayerInfo(idx, u);
            if (u.Id == ClientManager.instance.playerID)
            {
                myInfo = p.GetComponent<Player>();
                p.AddComponent<PlayerController>();

                roleText.text = isMafia ? "마피아" : "시민";
                StartInformation(string.Format("당신은 {0}입니다", roleText.text));

                Camera.main.transform.parent = p.transform.Find("Head");
                Camera.main.transform.localPosition = new Vector3(0, 0, 0);
            }

            players.Add(u.Id, p);
            idx++;
        }
    }

    private void GatherPlayers()
    {
        Transform spawnPos = GameObject.Find("SpawnPosition").transform;

        for (int i = 0; i < playerOrder.Count; i++)
        {
            if (playerOrder[i] == ClientManager.instance.playerID)
            {
                GameObject myObject = players[playerOrder[i]];
                myObject.GetComponent<PlayerController>().ControllerEnabled = false;
                myObject.transform.position = spawnPos.GetChild(i).position;
                myObject.GetComponent<PlayerController>().ControllerEnabled = true;

                ClientManager.instance.EmitMove(myObject.transform.position, myObject.transform.rotation);
            }
        }
    }

    public void KillPlayer(int deadID)
    {
        players[deadID].GetComponent<Player>().Dead();
    }

    #region Phase
    public void StartDay()
    {
        if (votingPanel)
        {
            OffVotingPanel();
        }

        StartInformation("낮이 되었습니다.");
        FadeInOut.instance.FadeAll();
    }
    public void StartNight()
    {
        phaseChange = true;

        //FadeInOut.instance.FadeIn();

        StartInformation("밤이 되었습니다.");
        FadeInOut.instance.FadeAll(() => { GatherPlayers(); }, () => { phaseChange = false; });
        //GatherPlayers();

        //phaseChange = false;
    }
    #endregion

    #region Information
    public void StartInformation(string s)
    {
        informationText.gameObject.SetActive(true);
        informationText.text = s;

        StopCoroutine("FadeOutInformationText");
        StartCoroutine("FadeOutInformationText");
    }
    private IEnumerator FadeOutInformationText()
    {
        Color orginColor = new Color(informationText.color.r, informationText.color.g, informationText.color.b, 1);
        Color clearColor = new Color(informationText.color.r, informationText.color.g, informationText.color.b, 0);
        float time = 0f;

        while (informationText.color.a > 0.0f)
        {
            informationText.color = Color.Lerp(orginColor, clearColor, time / fadeTime);
            time += Time.deltaTime;

            yield return null;
        }

        informationText.text = "";
        informationText.color = orginColor;
        informationText.gameObject.SetActive(false);
    }
    #endregion

    #region MenuPanel
    private void InitMenuPanel()
    {
        menuPanel.SetActive(true);

        menuPanel.transform.GetChild(0).Find("Back Button").GetComponent<Button>().onClick.AddListener(OnBackButton);

        menuPanel.SetActive(false);
    }
    private void SetActiveMenuPanel(bool state)
    {
        if (!isVoting)
        {
            if (state)
                ShowCursor();
            else
                HideCursor();
        }

        menuPanel.SetActive(state);
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
        if (suffrage && myInfo.IsAlive)
        {
            string s = playerObjects.transform.GetChild(pNum - 1).name;
            ClientManager.instance.EmitVoteReq(int.Parse(s.Replace("Player_", "")));
        }
    }

    private void UpdateVotingContent()
    {
        var votingContent = votingPanel.transform.GetChild(0);
        for (int i = 0; i < players.Count; i++)
        {
            if (!players[playerOrder[i]].GetComponent<Player>().IsAlive)
            {
                var btn = votingContent.GetChild(i + 1).GetComponent<Button>();
                btn.interactable = false;
            }
        }
    }

    public void OnVotingPanel(int time)
    {
        suffrage = true;
        isVoting = true;
        ShowCursor();

        votingPanel.SetActive(true);
        UpdateVotingContent();

        StartCoroutine(UpdateVotingTime(time));
    }
    IEnumerator UpdateVotingTime(int time)
    {
        timeText.enabled = true;
        while (0 < time)
        {
            timeText.text = time.ToString();

            time--;
            yield return new WaitForSeconds(1f);
        }
    }
    private void OffVotingPanel()
    {
        suffrage = false;
        isVoting = false;
        HideCursor();

        votingPanel.SetActive(false);
    }

    public void DisplayVotingResult(int electedId, (int pid, int count)[] result)
    {
        timeText.enabled = false;

        var votingContent = votingPanel.transform.GetChild(0);
        for (int i = 0; i < players.Count; i++)
        {
            votingContent.GetChild(i + 1).Find("Count Text").GetComponent<Text>().text = "";
        }

        for (int i = 0; i < result.Length; i++)
        {
            int id = result[i].pid;
            for (int j = 0; j < result.Length; j++)
            {
                if (id == playerOrder[j])
                {
                    Text countText = votingContent.GetChild(j + 1).Find("Count Text").GetComponent<Text>();
                    countText.text = result[i].count.ToString();
                    if (id == electedId)
                    {
                        votingContent.GetChild(j + 1).GetComponent<Button>().image.color = Color.red;
                    }

                    break;
                }
            }
        }

        if (electedId == -1)
        {
            StartInformation("투표가 부결되었습니다.");
        }
        else
        {
            StartInformation($"{players[electedId].GetComponent<Player>().Name}님이 {result[0].count}표를 받아 지목되었습니다.");
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
        if (suffrage && myInfo.IsAlive)
        {
            ClientManager.instance.EmitFinalVoteReq(isPros);
        }
    }

    public void OnFinalVotingPanel(int id)
    {
        isVoting = true;
        ShowCursor();

        for (int i = 0; i < players.Count; i++)
        {
            if (playerOrder[i] == id)
            {
                Transform imageTR = finalVotingPanel.transform.GetChild(0).Find("Image");
                imageTR.GetComponent<Image>().color = Global.colors[i];
                imageTR.GetComponentInChildren<Text>().text = players[id].GetComponent<Player>().Name;

                break;
            }
        }

        finalVotingPanel.SetActive(true);
    }
    public void StartDefense(int id, int defenseTime)
    {
        if (votingPanel)
        {
            OffVotingPanel();
        }

        OnFinalVotingPanel(id);
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
        isVoting = false;
        HideCursor();

        finalVotingPanel.SetActive(false);
    }

    public void DisplayFinalVotingResult(int id, int count)
    {
        OffFinalVotingPanel();

        if (id != -1)
        {
            Player p = players[id].GetComponent<Player>();

            StartInformation($"{p.Name}님이 {count}명의 동의로 추방되었습니다.");
            p.Dead();
        }
        else
        {
            StartInformation($"찬성 {count}표로 부결되었습니다.");
        }
    }
    #endregion
}
