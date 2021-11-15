using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class InGameUIManager : MonoBehaviour
{

    #region field
    public bool isVR;
    //text
    private Text roleText;
    private Text informationText;
    //ui
    private GameObject killUI;
    private Image killCheckUI;
    private Image killFillImage;
    private Image deadReportUI;
    private Image voiceUI;
    private FadeInOut fadeInOut;
    //panel
    private GameObject menuPanel;
    private VotingPanel votingPanel;
    private FinalVotingPanel finalVotingPanel;
    private GameObject winPanel;
    private GameObject losePanel;
    //Mision
    private Text missionText;
    private FirstMission firstMissionPanel;

    private readonly float fadeTime = 3f;
    #endregion

    #region property
    private PhotonManager pm => PhotonManager.Instance;
    private GameManager gm => GameManager.Instance;
    #endregion

    #region unity message
    private void Awake()
    {
        if (pm == null || PhotonNetwork.CurrentRoom == null)
        {
            SceneManager.LoadScene(isVR ? "IntroVR" : "Intro");
            return;
        }
        FindReference();
        Init();
    }
    private void Update()
    {
        if (isVR ? OVRInput.GetDown(OVRInput.Button.Start) : Input.GetKeyDown(KeyCode.Escape))
            OnMenuButton();
    }
    private void OnDestroy()
    {
        SetActiveCursor(true);
    }
    #endregion

    #region method
    private void FindReference()
    {
        //text
        roleText = transform.Find("Role Text").GetComponent<Text>();
        informationText = transform.Find("Information Text").GetComponent<Text>();
        //ui
        killUI = transform.Find("Kill UI").gameObject;
        killCheckUI = killUI.transform.Find("Kill Check UI").GetComponent<Image>();
        killFillImage = killUI.transform.Find("Fill Image").GetComponent<Image>();
        deadReportUI = transform.Find("DeadReport UI").GetComponent<Image>();
        voiceUI = transform.Find("Voice UI").GetComponent<Image>();
        fadeInOut = transform.GetComponentInChildren<FadeInOut>();
        //panel
        menuPanel = transform.Find("Menu Panel").gameObject;
        votingPanel = transform.Find("Voting Panel").GetComponent<VotingPanel>();
        finalVotingPanel = transform.Find("Final Voting Panel").GetComponent<FinalVotingPanel>();
        winPanel = transform.Find("Win Panel").gameObject;
        losePanel = transform.Find("Lose Panel").gameObject;
        //Mision
        missionText = transform.Find("Mission UI").GetComponentInChildren<Text>();
        firstMissionPanel = transform.Find("First Mission Panel").GetComponent<FirstMission>();
    }
    private void Init()
    {
        SetActiveCursor(false);
        votingPanel.gameObject.SetActive(true);
        finalVotingPanel.gameObject.SetActive(true);
        votingPanel.VoteButtonClicked += gm.OnVoteButton;
        finalVotingPanel.FinalVoteButtonClicked += gm.OnFinalVoteButton;
    }
    private void SetActiveCursor(bool active)
    {
        if (active)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else if (!gm.IsVoting)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    #region Information
    public void StartInformation(string s)
    {
        informationText.gameObject.SetActive(true);
        informationText.text = s;

        StopCoroutine(nameof(FadeOutInformationText));
        StartCoroutine(nameof(FadeOutInformationText));
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

    public void SetMissionText(string text)
    {
        missionText.text = text;
    }
    #endregion

    #region event handler

    #region game event
    public void OnGameStarted(bool isMafia, int[] mafiaIds)
    {
        roleText.text = isMafia ? "마피아" : "시민";
        if (!isMafia)
            killUI.SetActive(false);
        StartInformation(string.Format("당신은 {0}입니다", roleText.text));
        fadeInOut.FadeIn();
    }
    public void OnDayStarted()
    {
        SetActiveCursor(false);
        votingPanel.PanelOff();
        finalVotingPanel.PanelOff();
        StartInformation("낮이 되었습니다.");
        fadeInOut.FadeAll();
    }
    public void OnNightStarted()
    {
        StartInformation("밤이 되었습니다.");
        gm.PhaseChanging = true;
        fadeInOut.FadeAll(
            () =>
            {
                gm.ReturnSpawnPosition();
            },
            () =>
            {
                gm.PhaseChanging = false;
            });
    }
    public void OnVotingStarted(float votingTime)
    {
        SetActiveCursor(true);
        votingPanel.VotingStart(votingTime);
    }
    public void OnVotingEnded(int electedId, int[] result)
    {
        if (electedId == -1)
        {
            StartInformation("투표가 부결되었습니다.");
        }
        else
        {
            var elected = PhotonNetwork.CurrentRoom.Players[electedId];
            StartInformation($"{elected.NickName}님이 {result[Array.IndexOf(PhotonNetwork.PlayerList, elected)]}표를 받아 지목되었습니다.");
        }
        votingPanel.VotingEnd(electedId, result);
    }
    public void OnDefenseStarted(int electedId, float defenseTime)
    {
        votingPanel.PanelOff();
        finalVotingPanel.DefenseStart(electedId, defenseTime);
    }
    public void OnFinalVotingStarted(float finalVotingTime)
    {
        finalVotingPanel.FinalVotingStart(finalVotingTime);
    }
    public void OnFinalVotingEnded(int electedId, int pros)
    {
        var result = electedId != -1;
        finalVotingPanel.FinalVotingEnd(result);
        if (result)
            StartInformation($"{PhotonNetwork.CurrentRoom.Players[electedId].NickName}님이 {pros}명의 동의로 추방되었습니다.");
        else
            StartInformation($"찬성 {pros}표로 부결되었습니다.");
    }
    public void OnVoteFailed()
    {
        votingPanel.VoteFail();
    }
    public void OnFinalVoteFailed()
    {
        finalVotingPanel.FinalVoteFail();
    }
    public void OnKillResponse(float coolTime)
    {
        if (coolTime > 0)
            StartCoroutine(UpdateCoolTime(coolTime));
    }
    IEnumerator UpdateCoolTime(float coolTime)
    {
        float ct = coolTime;

        killFillImage.fillAmount = 1f;
        while (0 < ct)
        {
            ct -= 0.5f;
            killFillImage.fillAmount = ct / coolTime;

            yield return new WaitForSeconds(0.5f);
        }
        killFillImage.fillAmount = 0f;

    }
    public void OnGameEnded(bool win)
    {
        finalVotingPanel.PanelOff();
        if (win)
            winPanel.gameObject.SetActive(true);
        else
            losePanel.gameObject.SetActive(true);
        SetActiveCursor(true);
    }
    public void OnFoundTarget(Player target)
    {
        killCheckUI.color = Color.white;
        deadReportUI.color = Color.white;
        if (target != null)
        {
            if (target.GetAlive())
                killCheckUI.color = Color.red;
            else
                deadReportUI.color = Color.red;
        }
    }
    public void OnVoiceKey(bool userMute)
    {
        voiceUI.color = (userMute ? Color.gray : Color.white);
    }
    public void OnMissionStarted()
    {
        SetActiveCursor(true);
        firstMissionPanel.MissionStart();
    }
    #endregion

    #region button event
    public void OnMenuButton()
    {
        menuPanel.SetActive(!menuPanel.activeSelf);
        SetActiveCursor(menuPanel.activeSelf);
        gm.MenuOpened = menuPanel.activeSelf;
    }
    public void OnMenuBackButton()
    {
        menuPanel.SetActive(false);
        gm.MenuOpened = false;
    }
    public void OnOutGameButton()
    {
        PhotonManager.Instance.Dissconnect();
    }
    #endregion

    #endregion
}
