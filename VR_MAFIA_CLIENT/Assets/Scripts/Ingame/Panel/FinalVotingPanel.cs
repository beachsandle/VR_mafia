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
    private Text timeText;
    private Image electedImage;
    private Text electedName;
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
        timeText = content.Find("Time Text").GetComponent<Text>();
        electedImage = content.Find("Image").GetComponent<Image>();
        electedName = electedImage.GetComponentInChildren<Text>();
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
    public void FinalVotingStart(float finalVotingTime, Player elected)
    {
        Suffrage = true;
        timeText.enabled = true;
        electedImage.color = Global.colors[Array.IndexOf(playerList, elected)];
        electedName.text = elected.NickName;
        electedName.color = Color.white;
        gameObject.SetActive(true);
        StartCoroutine(UpdateVotingTime(finalVotingTime));
    }
    public void VoteFail()
    {
        Suffrage = true;
    }
    public void VotingEnd(bool result)
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
