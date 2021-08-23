using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviourPunCallbacks, IOnEventCallback
{
    private Transform[] spawnPositions;
    private Vector3 spawnPosition;
    private PlayerController localPlayerController;
    private RaiseEventOptions eventOption = new RaiseEventOptions() { Receivers = ReceiverGroup.All };
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
    public void OnEvent(EventData photonEvent)
    {
        switch ((VrMafiaEventCode)photonEvent.Code)
        {
            case VrMafiaEventCode.DayStart:
                Debug.Log($"[GameManager] Day Start");
                break;
            case VrMafiaEventCode.NightStart:
                Debug.Log($"[GameManager] Night Start");
                localPlayerController.MoveTo(spawnPosition);
                break;
            case VrMafiaEventCode.VotingStart:
                Debug.Log($"[GameManager] Voting Start");
                break;
            default: break;
        }
    }
}
