using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IntroManager : MonoBehaviour
{
    private static IntroManager _instance;
    public static IntroManager instance
    {
        get
        {
            if (!_instance)
            {
                _instance = FindObjectOfType(typeof(IntroManager)) as IntroManager;
            }

            return _instance;
        }
    }

    [SerializeField]
    private Text Text;

    [SerializeField]
    private Button connectButton;
    [SerializeField]
    private InputField hostIpInputField;
    [SerializeField]
    private InputField portInputField;

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }
    void Start()
    {
        connectButton.onClick.AddListener(OnConnectButton);
    }

    public void DisplayText(string s)
    {
        Text.text = s;
    }

    public void OnConnectButton()
    {
        FindObjectOfType<ClientManager>().gameObject.SetActive(true);

        ClientManager.instance.hostIp = hostIpInputField.text;
        ClientManager.instance.port = (hostIpInputField.text == "") ? 0 : int.Parse(hostIpInputField.text);

        ClientManager.instance.ConnectToServer();
    }
}
