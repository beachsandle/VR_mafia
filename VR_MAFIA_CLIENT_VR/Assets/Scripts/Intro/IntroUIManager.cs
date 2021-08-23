using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IntroUIManager : MonoBehaviour
{
    private InputField nicknameInput;
    private void Awake()
    {
        nicknameInput = transform.Find("IntroPanel").Find("Nickname").GetComponentInChildren<InputField>();
    }
    private void Start()
    {
        OnConnectButton();
    }
    public void OnConnectButton()
    {
        PhotonManager.Instance.Connect(nicknameInput.text);
    }
}
