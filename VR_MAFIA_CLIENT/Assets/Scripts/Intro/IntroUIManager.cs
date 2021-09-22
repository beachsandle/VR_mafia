using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VRKeyboard.Utils;

public class IntroUIManager : MonoBehaviour
{
    [SerializeField] private KeyboardManager keyboard;
    private InputField nicknameInput;

    private void Awake()
    {
        nicknameInput = transform.Find("IntroPanel").Find("Nickname").GetComponentInChildren<InputField>();

        if(keyboard)
        {
            InitKeyboard();
        }
    }
    private void Start()
    {
        //OnConnectButton();
    }
    private void Update()
    {
        if (!keyboard) return;

        if (nicknameInput.isFocused && !keyboard.gameObject.activeSelf)
        {
            keyboard.gameObject.SetActive(true);
        }
    }

    private void InitKeyboard()
    {
        Transform row;
        
        row = keyboard.keys.Find("row0");
        row.Find("Back").GetComponent<Button>().onClick.AddListener(keyboard.Backspace);
        
        row = keyboard.keys.Find("row2");
        row.Find("CapsLock").GetComponent<Button>().onClick.AddListener(keyboard.CapsLock);

        row = keyboard.keys.Find("row3");
        row.Find("Shift").GetComponent<Button>().onClick.AddListener(keyboard.Shift);

        row = keyboard.keys.Find("row4");
        row.Find("Clear").GetComponent<Button>().onClick.AddListener(keyboard.Clear);
        row.Find("Enter").GetComponent<Button>().onClick.AddListener(() => OnEnterKey(keyboard.inputText));
    }

    public void OnEnterKey(Text text)
    {
        nicknameInput.text = text.text;
        keyboard.gameObject.SetActive(false);
    }
    public void OnConnectButton()
    {
        PhotonManager.Instance.Connect(nicknameInput.text);
    }
}
