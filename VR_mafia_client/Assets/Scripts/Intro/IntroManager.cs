using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IntroManager : MonoBehaviour
{
    [SerializeField] private bool autoBegin;
    [SerializeField] private Text informationText;
    [SerializeField] private InputField hostIpInputField;
    [SerializeField] private InputField portInputField;
    [SerializeField] private Button connectButton;

    void Start()
    {
        connectButton.onClick.AddListener(OnConnectButton);
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

    public void DisplayText(string s)
    {
        informationText.text = s;
    }

    public void OnConnectButton()
    {
        ClientManager.Instance.hostIp = hostIpInputField.text;
        ClientManager.Instance.port = (portInputField.text == "") ? 0 : int.Parse(portInputField.text);

        if (!ClientManager.Instance.ConnectToServer())
            DisplayText("서버와 연결에 실패했습니다.");
    }
}
