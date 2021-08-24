using Photon.Pun;
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
    private GameObject deadReportUI;
    private FadeInOut fadeInOut;
    //panel
    private GameObject menuPanel;
    private GameObject votingPanel;
    private GameObject finalVotingPanel;
    private GameObject endPanel;

    private float fadeTime = 3f;
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
        deadReportUI = transform.Find("DeadReport UI").gameObject;
        fadeInOut = transform.GetComponentInChildren<FadeInOut>();
        //panel
        menuPanel = transform.Find("Menu Panel").gameObject;
        votingPanel = transform.Find("Voting Panel").gameObject;
        finalVotingPanel = transform.Find("Final Voting Panel").gameObject;
        endPanel = transform.Find("End Panel").gameObject;
    }
    private void Init()
    {
        SetActiveCursor(false);
        gm.GameStarted += OnGameStarted;
        gm.DayStarted += OnDayStarted;
        gm.NightStarted += OnNightStarted;
    }

    private void OnGameStarted(bool isMafia, int[] mafiaIds)
    {
        roleText.text = isMafia ? "마피아" : "시민";
        if (isMafia) { }
        else
        {
            killUI.SetActive(false);
        }

        StartInformation(string.Format("당신은 {0}입니다", roleText.text));
        fadeInOut.FadeIn();
    }

    private void OnNightStarted()
    {
        StartInformation("밤이 되었습니다.");
        fadeInOut.FadeAll(
            () =>
            {
                gm.ReturnSpawnPosition();
            },
            () => { });
    }

    private void OnDayStarted()
    {
        if (votingPanel.activeSelf)
            OffVotingPanel();
        StartInformation("낮이 되었습니다.");
        fadeInOut.FadeAll();
    }

    private void OffVotingPanel()
    {
        //suffrage = false;
        //isVoting = false;
        SetActiveCursor(false);

        votingPanel.SetActive(false);
    }
    private void SetActiveCursor(bool active)
    {
        if (active)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
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

    #endregion

    #region event handler

    #region button event
    public void OnMenuButton()
    {
        menuPanel.SetActive(!menuPanel.activeSelf);
        SetActiveCursor(menuPanel.activeSelf);
    }
    public void OnMenuBackButton()
    {
        menuPanel.SetActive(false);
    }
    #endregion

    #endregion
}
