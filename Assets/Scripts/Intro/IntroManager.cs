using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IntroManager : MonoBehaviour
{
    private InputField nicknameInput;
    private void Awake()
    {
        nicknameInput = transform.Find("IntroPanel").Find("Nickname").GetComponentInChildren<InputField>();
    }
    public void OnConnectButton()
    {
        if (nicknameInput.text != "")
            PhotonManager.Instance.Connect(nicknameInput.text);
    }
}
