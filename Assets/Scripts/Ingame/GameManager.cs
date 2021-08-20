using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviourPunCallbacks, IOnEventCallback
{
    private Transform[] spawnPositions;
    private RaiseEventOptions eventOption = new RaiseEventOptions() { Receivers = ReceiverGroup.All };
    private void Awake()
    {
        spawnPositions = transform.Find("SpawnPosition").GetComponentsInChildren<Transform>();
    }
    void Start()
    {
        SpawnPlayer();
        if (PhotonNetwork.IsMasterClient)
            StartCoroutine(GameLogic());
    }
    private void SpawnPlayer()
    {
        var player = PhotonNetwork.Instantiate("Player_SE", spawnPositions[PhotonNetwork.LocalPlayer.ActorNumber].position, Quaternion.identity);
        var controller = player.AddComponent<PlayerController>();
        controller.SetCamera(Camera.main);
    }
    private IEnumerator GameLogic()
    {
        while (true)
        {
            PhotonNetwork.RaiseEvent(1, null, eventOption, SendOptions.SendReliable);
            yield return new WaitForSeconds(3f);
        }
    }

    public void OnEvent(EventData photonEvent)
    {
        if (photonEvent.Code == 1)
            Debug.Log(photonEvent.Code);
    }
}
