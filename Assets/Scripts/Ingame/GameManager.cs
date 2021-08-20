using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private Transform[] spawnPositions;
    private void Awake()
    {
        spawnPositions = transform.Find("SpawnPosition").GetComponentsInChildren<Transform>();
    }
    void Start()
    {
        var player = PhotonNetwork.Instantiate("Player_SE", spawnPositions[PhotonNetwork.LocalPlayer.ActorNumber].position, Quaternion.identity);
        var controller = player.AddComponent<PlayerController>();
        controller.SetCamera(Camera.main);
    }
}
