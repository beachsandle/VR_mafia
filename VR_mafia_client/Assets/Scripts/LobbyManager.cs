using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LobbyManager : MonoBehaviour
{
    [SerializeField] private GameObject roomList;
    [Header("UI")]
    [SerializeField] private Text playerName;

    [Header("Button")]
    [SerializeField] private Button joinButton;
    [SerializeField] private Button createButton;
    [SerializeField] private Button refreshButton;
    [SerializeField] private Button changeNameButton;

    [Header("ChangeNamePanel")]
    [SerializeField] private GameObject changeNamePanel;
    [SerializeField] private InputField inputField;
    [SerializeField] private Button okButton;
    [SerializeField] private Button noButton;

    void Start()
    {
        createButton.onClick.AddListener(OnCreateButton);

        InitChangeNamePanel();
    }

    void OnJoinButton()
    {

    }

    void OnCreateButton()
    {
        roomList.GetComponent<RoomList>().CreateRoom();
    }

    void OnRefreshButton()
    {

    }

    #region ChangeNamePanel
    void InitChangeNamePanel()
    {
        changeNamePanel.SetActive(true);

        changeNameButton.onClick.AddListener(OnChangeNameButton);
        okButton.onClick.AddListener(OnOKButton);
        noButton.onClick.AddListener(OnNoButton);

        inputField.characterLimit = 10;

        changeNamePanel.SetActive(false);
    }
    void OnChangeNameButton()
    {
        changeNamePanel.SetActive(true);
        inputField.text = playerName.text;
    }
    void OnOKButton()
    {
        playerName.text = inputField.text;
        changeNamePanel.SetActive(false);
    }
    void OnNoButton()
    {
        changeNamePanel.SetActive(false);
    }
    #endregion
}
