using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System;
using Photon.Pun;
using System.Linq;
using Photon.Realtime;

public class VotingPanel : MonoBehaviour
{
    #region field
    private Transform content;
    private Button[] btns;
    private Text timeText;
    private int[] playerIds;
    #endregion

    #region property
    private Player[] playerList => PhotonNetwork.PlayerList;
    private Dictionary<int, Player> players => PhotonNetwork.CurrentRoom.Players;
    public bool Suffrage { get; private set; } = false;
    #endregion

    #region callback
    public event Action<int> VoteButtonClicked;
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
        content = transform.Find("Voting Image");
        timeText = content.Find("Time Text").GetComponent<Text>();
        btns = content.GetComponentsInChildren<Button>();
    }
    private void Init()
    {
        for (int i = 0; i < 10; ++i)
        {
            int n = i;
            btns[i].onClick.AddListener(() =>
            {
                OnVoteButton(n);
            });
        }
        gameObject.SetActive(false);
    }
    private void InitCandidates()
    {
        playerIds = playerList.Select(p => p.ActorNumber).ToArray();
        for (int i = 0; i < 10; ++i)
        {
            if (i < playerList.Length)
            {
                btns[i].gameObject.SetActive(true);
                btns[i].transform.GetComponent<Image>().color = Global.colors[i];
                var nameText = btns[i].transform.Find("Text").GetComponent<Text>();
                nameText.text = playerList[i].NickName;
                nameText.color = Color.white;
                btns[i].transform.Find("Count Text").GetComponent<Text>().text = "0";
                btns[i].transform.Find("Selected UI").gameObject.SetActive(false);

                if (!playerList[i].Alive())
                    btns[i].interactable = false;
            }
            else
                btns[i].gameObject.SetActive(false);
        }
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
    public void VotingStart(float votingTime)
    {
        Suffrage = true;
        timeText.enabled = true;
        gameObject.SetActive(true);
        InitCandidates();
        StartCoroutine(UpdateVotingTime(votingTime));
    }
    public void VoteFail()
    {
        Suffrage = true;
    }
    public void VotingEnd(int electedId, int[] result)
    {
        timeText.enabled = false;
        for (int i = 0; i < result.Length; ++i)
        {
            btns[i].transform.Find("Count Text").GetComponent<Text>().text = result[i].ToString();
        }
        if (electedId != -1)
        {
            int idx = Array.IndexOf(playerList, players[electedId]);
            btns[idx].transform.Find("Text").GetComponent<Text>().color = Color.red;
            btns[idx].transform.Find("Selected UI").gameObject.SetActive(true);
        }
    }
    public void PanelOff()
    {
        gameObject.SetActive(false);
    }
    #endregion

    #endregion

    #region event handler
    private void OnVoteButton(int number)
    {
        if (!Suffrage)
            return;
        Suffrage = false;
        VoteButtonClicked?.Invoke(playerIds[number]);
    }
    #endregion
}
