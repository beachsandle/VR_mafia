using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MyPacket;

public class InGameManager : MonoBehaviour
{
    private MySocket socket;

    private static InGameManager instance;
    public static InGameManager Instance
    {
        get
        {
            if (!instance)
            {
                instance = FindObjectOfType(typeof(InGameManager)) as InGameManager;
            }

            return instance;
        }
    }

    //private Global.GameStatus gameStatus;

    [SerializeField]
    private GameObject playerObj;

    private GameObject playerObjects;
    private bool isMafia;
    public bool phaseChange;
    public bool isVoting;
    public bool suffrage;
    private Transform spawnPos;

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

    private List<Player> players;
    private Dictionary<int, Player> playerDict; // id, object
    private Player myInfo;


    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        socket = ClientManager.Instance.Socket;

        SpawnPlayers();

        InitInGameEvent();
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
    void InitInGameEvent()
    {
        socket.On(PacketType.MOVE_EVENT, OnMoveEvent);
        socket.On(PacketType.DAY_START, OnDayStart);
        socket.On(PacketType.NIGHT_START, OnNightStart);
        socket.On(PacketType.START_VOTING, OnStartVoting);
        socket.On(PacketType.VOTE_RES, OnVoteRes);
        socket.On(PacketType.VOTING_RESULT, OnVotingResult);
        socket.On(PacketType.START_DEFENSE, OnStartDefense);
        socket.On(PacketType.START_FINAL_VOTING, OnStartFinalVoting);
        socket.On(PacketType.FINAL_VOTE_RES, OnFinalVoteRes);
        socket.On(PacketType.FINAL_VOTING_RESULT, OnFinalVotingResult);
        socket.On(PacketType.KILL_RES, OnKillRes);
        socket.On(PacketType.DIE_EVENT, OnDieEvent);
    }
    void ClearInGameEvent()
    {
        socket.Clear(PacketType.MOVE_EVENT);
        socket.Clear(PacketType.DAY_START);
        socket.Clear(PacketType.NIGHT_START);
        socket.Clear(PacketType.START_VOTING);
        socket.Clear(PacketType.VOTE_RES);
        socket.Clear(PacketType.VOTING_RESULT);
        socket.Clear(PacketType.START_DEFENSE);
        socket.Clear(PacketType.START_FINAL_VOTING);
        socket.Clear(PacketType.FINAL_VOTE_RES);
        socket.Clear(PacketType.FINAL_VOTING_RESULT);
        socket.Clear(PacketType.KILL_RES);
        socket.Clear(PacketType.DIE_EVENT);
    }

    #region InGame Event
    private void OnMoveEvent(Packet packet)
    {
        var data = new MoveEventData(packet.Bytes);

        UpdatePlayerTransform(data);
    }
    public void EmitMoveReq(Vector3 pos, Quaternion rot)
    {
        socket.Emit(PacketType.MOVE_REQ, new MoveReqData(MakeLocation(pos, rot)).ToBytes());
    }
    private Location MakeLocation(Vector3 pos, Quaternion rot)
    {
        var position = new V3(pos.x, pos.y, pos.z);
        var rotation = new V3(rot.eulerAngles.x, rot.eulerAngles.y, rot.eulerAngles.z);

        return new Location(position, rotation);
    }

    private void OnDayStart(Packet packet)
    {
        StartDay();
    }
    private void OnNightStart(Packet packet)
    {
        StartNight();
    }

    private void OnStartVoting(Packet packet)
    {
        var data = new StartVotingData(packet.Bytes);

        OnVotingPanel(data.Voting_time);
    }
    public void EmitVoteReq(int targetID)
    {
        socket.Emit(PacketType.VOTE_REQ, new VoteReqData(targetID).ToBytes());
    }
    private void OnVoteRes(Packet packet)
    {
        var data = new VoteResData(packet.Bytes);

        if (data.Result)
        {
            suffrage = false;
        }
    }
    private void OnVotingResult(Packet packet)
    {
        var data = new VotingResultData(packet.Bytes);

        DisplayVotingResult(data.elected_id, data.result);
    }

    private void OnStartDefense(Packet packet)
    {
        var data = new StartDefenseData(packet.Bytes);

        StartDefense(data.Elected_id, data.Defense_time);
    }

    private void OnStartFinalVoting(Packet packet)
    {
        var data = new StartFinalVotingData(packet.Bytes);

        StartFinalVoting(data.Voting_time);
    }
    public void EmitFinalVoteReq(bool agree)
    {
        socket.Emit(PacketType.FINAL_VOTE_REQ, new FinalVoteReqData(agree).ToBytes());
    }
    private void OnFinalVoteRes(Packet packet)
    {
        var data = new FinalVoteResData(packet.Bytes);

        if (data.Result)
        {
            suffrage = false;
        }
    }
    private void OnFinalVotingResult(Packet packet)
    {
        var data = new FinalVotingResultData(packet.Bytes);

        DisplayFinalVotingResult(data.Kicking_id, data.voteCount);
    }

    public void EmitKillReq(int targetID)
    {
        socket.Emit(PacketType.KILL_REQ, new KillReqDada(targetID).ToBytes());
    }
    private void OnKillRes(Packet packet)
    {
        var data = new KillResDada(packet.Bytes);

        if (data.Result)
        {
            // 총알 없애기
        }
    }
    private void OnDieEvent(Packet packet)
    {
        var data = new DieEventData(packet.Bytes);

        KillPlayer(data.Dead_id);
    }
    #endregion

    public void UpdatePlayerTransform(MoveEventData data)
    {
        if (data.Player_id == SceneLoader.Instance.PlayerId) return;

        V3 pos = data.Location.position;
        V3 rot = data.Location.rotation;

        Transform TR = playerDict[data.Player_id].transform;
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
    private void SpawnPlayer(int idx, UserInfo u)
    {
        var p = Instantiate(playerObj).GetComponent<Player>();
        p.InitPlayerInfo(idx, u);

        p.name = "Player_" + u.Id;
        p.transform.position = spawnPos.GetChild(idx).position;
        p.transform.parent = playerObjects.transform;

        players.Add(p);
        playerDict[u.Id] = p;
    }
    private void SetMyPlayer(int myId)
    {
        myInfo = playerDict[myId];
        myInfo.gameObject.AddComponent<PlayerController>();

        Camera.main.transform.parent = myInfo.transform.Find("Head");
        Camera.main.transform.localPosition = new Vector3(0, 0, 0);
    }
    private void SpawnPlayers()
    {
        players = new List<Player>();
        playerDict = new Dictionary<int, Player>();
        spawnPos = GameObject.Find("SpawnPosition").transform;
        playerObjects = GameObject.Find("PlayerObjects");

        (var myId, var isMafia, var mafias, var userInfos) = SceneLoader.Instance.GetInGameInfo();
        this.isMafia = isMafia;

        for (int i = 0; i < userInfos.Count; i++)
            SpawnPlayer(i, userInfos[i]);

        SetMyPlayer(myId);

        if (isMafia)
        {
            foreach (int mid in mafias)
                playerDict[mid].IsMafia = true;
        }

        roleText.text = isMafia ? "마피아" : "시민";
        StartInformation(string.Format("당신은 {0}입니다", roleText.text));
    }
    public void KillPlayer(int deadID)
    {
        playerDict[deadID].GetComponent<Player>().Dead();
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

        StartInformation("밤이 되었습니다.");
        FadeInOut.instance.FadeAll(
            () =>
            {
                myInfo.GetComponent<PlayerController>().MoveTo(spawnPos.GetChild(myInfo.Number - 1).position);
            },
            () => { phaseChange = false; });
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
            if (pNum <= playerDict.Count)
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
            int targetID = players[pNum - 1].ID;
            EmitVoteReq(targetID);
        }
    }

    private void UpdateVotingContent()
    {
        var votingContent = votingPanel.transform.GetChild(0);
        for (int i = 0; i < playerDict.Count; i++)
        {
            if (!players[i].GetComponent<Player>().IsAlive)
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
        for (int i = 0; i < playerDict.Count; i++)
        {
            votingContent.GetChild(i + 1).Find("Count Text").GetComponent<Text>().text = "";
        }

        foreach ((int id, int count) in result)
        {
            var p = playerDict[id];
            Text countText = votingContent.GetChild(p.Number).Find("Count Text").GetComponent<Text>();
            countText.text = count.ToString();
            if (id == electedId)
            {
                votingContent.GetChild(p.Number).GetComponent<Button>().image.color = Color.red;
            }
        }

        if (electedId == -1)
        {
            StartInformation("투표가 부결되었습니다.");
        }
        else
        {
            StartInformation($"{playerDict[electedId].GetComponent<Player>().Name}님이 {result[0].count}표를 받아 지목되었습니다.");
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
            EmitFinalVoteReq(isPros);
        }
    }

    public void OnFinalVotingPanel(int id)
    {
        isVoting = true;
        ShowCursor();

        Transform imageTR = finalVotingPanel.transform.GetChild(0).Find("Image");
        imageTR.GetComponent<Image>().color = Global.colors[playerDict[id].Number-1];
        imageTR.GetComponentInChildren<Text>().text = playerDict[id].Name;

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
            Player p = playerDict[id].GetComponent<Player>();

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
