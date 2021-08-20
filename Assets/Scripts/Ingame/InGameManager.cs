using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InGameManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        if (PhotonManager.Instance == null || PhotonNetwork.CurrentRoom == null)
        {
            SceneManager.LoadScene("Intro");
            return;
        }
        var player = PhotonNetwork.Instantiate("Player_SE", Vector3.zero, Quaternion.identity);
        player.AddComponent<PlayerController>();
        Camera.main.transform.parent = player.transform;
        Camera.main.transform.localPosition = new Vector3(0, 2);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
