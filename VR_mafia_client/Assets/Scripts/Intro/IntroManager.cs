using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IntroManager : MonoBehaviour
{
    [SerializeField] private bool autoBegin;
    [SerializeField] private InputField hostIpInputField;
    [SerializeField] private InputField portInputField;
    [SerializeField] private Button connectButton;

    [SerializeField] private string ip;
    [SerializeField] private string port;

    void Start()
    {
        connectButton.onClick.AddListener(OnConnectButton);
        hostIpInputField.text = ip;
        portInputField.text = port;
        if (autoBegin)
            OnConnectButton();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            OnConnectButton();
        }
    }

    public void OnConnectButton()
    {
        ClientManager.Instance.hostIp = hostIpInputField.text;
        ClientManager.Instance.port = (portInputField.text == "") ? 0 : int.Parse(portInputField.text);

        ClientManager.Instance.ConnectToServer();
    }
}
