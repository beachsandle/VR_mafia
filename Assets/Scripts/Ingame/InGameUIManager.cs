using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class InGameUIManager : MonoBehaviour
{

    #region field

    #region reference
    //text
    private Text roleText;
    private Text informationText;
    //ui
    private GameObject killUI;
    private GameObject deadReportUI;
    //panel
    private GameObject menuPanel;
    private GameObject votingPanel;
    private GameObject finalVotingPanel;
    private GameObject endPanel;
    #endregion

    #endregion

    #region property
    private PhotonManager pm => PhotonManager.Instance;
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
        //panel
        menuPanel = transform.Find("Menu Panel").gameObject;
        votingPanel = transform.Find("Voting Panel").gameObject;
        finalVotingPanel = transform.Find("Final Voting Panel").gameObject;
        endPanel = transform.Find("End Panel").gameObject;
    }
    private void Init()
    {
        var player = PhotonNetwork.Instantiate("Player_SE", Vector3.zero, Quaternion.identity);
        player.AddComponent<PlayerController>();
        Camera.main.transform.parent = player.transform;
        Camera.main.transform.localPosition = new Vector3(0, 2);
    }
    #endregion

    #region event handler

    #region button event
    public void OnMenuButton()
    {
        menuPanel.SetActive(!menuPanel.activeSelf);
    }
    public void OnMenuBackButton()
    {
        menuPanel.SetActive(false);
    }
    public void OnVoteButton(int actNum) { }
    public void OnFinalVoteButton(bool pros) { }
    public void OnLobbyButton() { }

    #endregion

    #endregion
}
