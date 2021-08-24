using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System;
using Photon.Pun;
using System.Linq;
using Photon.Realtime;

public class FinalVotingPanel : MonoBehaviour
{
    #region field
    private Transform content;
    private Text subjectText;
    private Text timeText;
    private Image electedImage;
    private Text electedName;
    private Transform prosButton;
    private Transform consButton;
    #endregion

    #region property
    private Player[] playerList => PhotonNetwork.PlayerList;
    private Dictionary<int, Player> players => PhotonNetwork.CurrentRoom.Players;
    public bool Suffrage { get; private set; } = false;
    #endregion

    #region callback
    public event Action<bool> FinalVoteButtonClicked;
    #endregion

    #region unity message
    private void Awake()
    {
        FindReference();
        Init();
    }
    #endregion

    #region method

    #region private
    private void FindReference()
    {
        content = transform.Find("Final Voting Image");
        subjectText = content.Find("Subject Text").GetComponent<Text>();
        timeText = content.Find("Time Text").GetComponent<Text>();
        electedImage = content.Find("Image").GetComponent<Image>();
        electedName = electedImage.GetComponentInChildren<Text>();
        prosButton = content.Find("Pros Button");
        consButton = content.Find("Cons Button");
    }
    private void Init()
    {
        gameObject.SetActive(false);
    }
    private IEnumerator UpdateVotingTime(float votingTime)
    {
        timeText.enabled = true;
        while (votingTime > 0)
        {
            timeText.text = $"{(int)votingTime}";
            votingTime -= 0.5f;
            yield return new WaitForSeconds(0.5f);
        }
    }
    #endregion

    #region public
    public void DefenseStart(float defenseTime, int electedId)
    {
        subjectText.text = "Defense";
        timeText.enabled = true;
        var elected = players[electedId];
        electedImage.color = Global.colors[Array.IndexOf(playerList, elected)];
        electedName.text = elected.NickName;
        electedName.color = Color.white;
        prosButton.gameObject.SetActive(false);
        consButton.gameObject.SetActive(false);
        gameObject.SetActive(true);
        StartCoroutine(UpdateVotingTime(defenseTime));
    }
    public void FinalVotingStart(float finalVotingTime)
    {
        Suffrage = true;
        subjectText.text = "FinalVote";
        prosButton.gameObject.SetActive(true);
        consButton.gameObject.SetActive(true);
        StartCoroutine(UpdateVotingTime(finalVotingTime));
    }
    public void FinalVoteFail()
    {
        Suffrage = true;
    }
    public void FinalVotingEnd(bool result)
    {
        timeText.enabled = false;
        if (result)
            electedName.color = Color.red;
    }
    public void PanelOff()
    {
        gameObject.SetActive(false);
    }
    #endregion

    #endregion

    #region event handler
    public void OnFinalVoteButton(bool pros)
    {
        if (!Suffrage)
            return;
        Suffrage = false;
        FinalVoteButtonClicked?.Invoke(pros);
    }
    #endregion
}
