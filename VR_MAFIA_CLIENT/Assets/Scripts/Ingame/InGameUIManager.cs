﻿using Photon.Pun;
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
    //text
    private Text roleText;
    private Text informationText;
    //ui
    private GameObject killUI;
    private Image killCheckUI;
    private Image deadReportUI;
    private FadeInOut fadeInOut;
    //panel
    private GameObject menuPanel;
    private VotingPanel votingPanel;
    private FinalVotingPanel finalVotingPanel;
    private GameObject endPanel;

    private int electedId;
    private bool canKill = false;
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
            SceneManager.LoadScene("Intro");
            return;
        }
        FindReference();
        Init();
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            OnMenuButton();
    }
    private void OnDestroy()
    {
        SetActiveCursor(true);
        if (pm != null)
        {
            gm.GameStarted -= OnGameStarted;
            gm.DayStarted -= OnDayStarted;
            gm.NightStarted -= OnNightStarted;
            gm.VotingStarted -= OnVotingStarted;
            gm.VoteFailed -= votingPanel.VoteFail;
            gm.FinalVoteFailed -= finalVotingPanel.FinalVoteFail;
            gm.KillFailed -= OnKillFailed;
            gm.VotingEnded -= OnVotingEnded;
            gm.DefenseStarted -= OnDefenseStarted;
            gm.FinalVotingStarted -= OnFinalVotingStarted;
            gm.FinalVotingEnded -= OnFinalVotingEnded;
            gm.GameEnded -= OnGameEnded;
        }
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
        deadReportUI = transform.Find("DeadReport UI").GetComponent<Image>();
        fadeInOut = transform.GetComponentInChildren<FadeInOut>();
        //panel
        menuPanel = transform.Find("Menu Panel").gameObject;
        votingPanel = transform.Find("Voting Panel").GetComponent<VotingPanel>();
        finalVotingPanel = transform.Find("Final Voting Panel").GetComponent<FinalVotingPanel>();
        endPanel = transform.Find("End Panel").gameObject;
    }
    private void Init()
    {
        SetActiveCursor(false);
        EnrollHandler();
        votingPanel.gameObject.SetActive(true);
        finalVotingPanel.gameObject.SetActive(true);
    }
    private void EnrollHandler()
    {
        gm.FoundTarget += OnFoundTarget;
        gm.GameStarted += OnGameStarted;
        gm.DayStarted += OnDayStarted;
        gm.NightStarted += OnNightStarted;
        gm.VotingStarted += OnVotingStarted;
        gm.VoteFailed += votingPanel.VoteFail;
        gm.FinalVoteFailed += finalVotingPanel.FinalVoteFail;
        gm.KillFailed += OnKillFailed;
        gm.VotingEnded += OnVotingEnded;
        gm.DefenseStarted += OnDefenseStarted;
        gm.FinalVotingStarted += OnFinalVotingStarted;
        gm.FinalVotingEnded += OnFinalVotingEnded;
        gm.GameEnded += OnGameEnded;
        votingPanel.VoteButtonClicked += gm.OnVoteButton;
        finalVotingPanel.FinalVoteButtonClicked += gm.OnFinalVoteButton;

    }


    private void OnFoundTarget(Player target)
    {
        killCheckUI.color = Color.white;
        deadReportUI.color = Color.white;
        if (target != null)
        {
            if (target.Alive())
                killCheckUI.color = Color.red;
            else
                deadReportUI.color = Color.red;
        }
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

    #endregion

    #region event handler

    #region game event
    private void OnGameStarted(bool isMafia, int[] mafiaIds)
    {
        roleText.text = isMafia ? "마피아" : "시민";
        if (isMafia)
        {
            canKill = true;
        }
        else
        {
            killUI.SetActive(false);
        }

        StartInformation(string.Format("당신은 {0}입니다", roleText.text));
        fadeInOut.FadeIn();
    }
    private void OnDayStarted()
    {
        SetActiveCursor(false);
        votingPanel.PanelOff();
        finalVotingPanel.PanelOff();
        StartInformation("낮이 되었습니다.");
        fadeInOut.FadeAll();
    }
    private void OnNightStarted()
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
    private void OnVotingStarted(float votingTime)
    {
        SetActiveCursor(true);
        votingPanel.VotingStart(votingTime);
    }
    private void OnVotingEnded(int electedId, int[] result)
    {
        this.electedId = electedId;
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
    private void OnDefenseStarted(float defenseTime)
    {
        finalVotingPanel.DefenseStart(defenseTime, electedId);
    }
    private void OnFinalVotingStarted(float finalVotingTime)
    {
        finalVotingPanel.FinalVotingStart(finalVotingTime);
    }
    private void OnFinalVotingEnded(bool result, int pros)
    {
        finalVotingPanel.FinalVotingEnd(result);
        if (result)
            StartInformation($"{PhotonNetwork.CurrentRoom.Players[electedId].NickName}님이 {pros}명의 동의로 추방되었습니다.");
        else
            StartInformation($"찬성 {pros}표로 부결되었습니다.");
    }
    private void OnKillFailed()
    {
        //Todo: kill 실패
    }
    private void OnGameEnded(bool mafiaWin, int[] mafiaIds)
    {
        //Todo: 게임 종료 패널
        endPanel.gameObject.SetActive(true);
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
    #endregion

    #endregion
}
