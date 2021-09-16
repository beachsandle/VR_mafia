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
        keyboard.keys.Find("row4").Find("Enter").GetComponent<Button>().onClick.AddListener(() => OnEnterKey(keyboard.inputText));
        keyboard.keys.Find("row0").Find("Back").GetComponent<Button>().onClick.AddListener(keyboard.Backspace);
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
