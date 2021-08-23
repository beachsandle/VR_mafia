using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviourPunCallbacks, IOnEventCallback
{
    #region singleton
    private static GameManager instance;
    public static GameManager Instance
    {
        get
        {
            if (instance == null)
                instance = FindObjectOfType<GameManager>();
            return instance;
        }
    }
    #endregion

    private Transform[] spawnPositions;
    private Vector3 spawnPosition;
    private PlayerController localPlayerController;
    private RaiseEventOptions eventOption = new RaiseEventOptions() { Receivers = ReceiverGroup.All };

    public event Action DayStarted;
    public event Action NightStarted;
    private void Awake()
    {
        spawnPositions = transform.Find("SpawnPosition").GetComponentsInChildren<Transform>();
    }
    void Start()
    {
        SpawnPlayer();
    }
    private void SpawnPlayer()
    {
        spawnPosition = spawnPositions[PhotonNetwork.LocalPlayer.ActorNumber].position;
        var player = PhotonNetwork.Instantiate("Player_SE", spawnPosition, Quaternion.identity);
        localPlayerController = player.AddComponent<PlayerController>();
        localPlayerController.SetCamera(Camera.main);
    }
    private void SetMafia(EventData data)
    {
        Debug.Log($"[GameManager] Set Mafia : {(int[])data.CustomData}");
    }
    private void DayStart()
    {
        Debug.Log($"[GameManager] Day Start");
        DayStarted?.Invoke();
    }
    private void NightStart()
    {
        Debug.Log($"[GameManager] Night Start");
        NightStarted?.Invoke();
    }
    private void VotingStart(EventData data)
    {
        Debug.Log($"[GameManager] Voting Start : {data.CustomData}");
    }
    public void OnEvent(EventData photonEvent)
    {
        switch ((VrMafiaEventCode)photonEvent.Code)
        {
            case VrMafiaEventCode.SetMafia:
                SetMafia(photonEvent);
                break;
            case VrMafiaEventCode.DayStart:
                DayStart();
                break;
            case VrMafiaEventCode.NightStart:
                NightStart();
                break;
            case VrMafiaEventCode.VotingStart:
                VotingStart(photonEvent);
                break;
            default: break;
        }
    }
    public void ReturnSpawnPosition() => localPlayerController.MoveTo(spawnPosition);
    public void OnVoteButton(int id)
    {

    }
    public void OnKillButton(int id) { }
    public void OnDeadReportButton(int id) { }
    public void OnFinalVoteButton(bool pros)
    {

    }
}
