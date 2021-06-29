using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyPacket;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    private static SceneLoader instance;
    public static SceneLoader Instance
    {
        get
        {
            if (!instance)
            {
                instance = FindObjectOfType(typeof(SceneLoader)) as SceneLoader;
            }

            return instance;
        }
    }

    public int PlayerId { get; private set; }
    public string UserName { get; private set; }
    public string RoomName { get; private set; }
    public bool IsMafia { get; private set; }
    public List<UserInfo> Users { get; private set; }
    public int[] Mafias { get; private set; }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        PlayerId = -1;
        UserName = "";
        RoomName = "";
        Users = new List<UserInfo>();
    }
    public void IntroToLobby(int pid)
    {
        PlayerId = pid;
        UserName = "Player" + PlayerId;
        PhotonManager.Instance.Connect();
        SceneManager.LoadScene("Lobby");
    }

    public void LobbyToWaitingRoom(string userName, int roomId, string roomName, List<UserInfo> users = null)
    {
        UserName = userName;
        RoomName = roomName;
        if (users == null)
            Users.Add(new UserInfo(PlayerId, userName));
        else
            Users = users;

        PhotonManager.Instance.JoinRoom(roomId);
        SceneManager.LoadScene("WaitingRoom");
    }
    public (string, List<UserInfo>) GetWaitingRoomInfo()
    {
        return (RoomName, Users);
    }
    public void WaitingRoomToLobby()
    {
        Users.Clear();
        PhotonManager.Instance.LeaveRoom();
        SceneManager.LoadScene("Lobby");
    }
    public void WaitingRoomToInGame(bool isMafia, int[] mafias, List<UserInfo> users)
    {
        IsMafia = isMafia;
        Users = users;
        Mafias = mafias;
        SceneManager.LoadScene("InGame");
    }
    public (int, bool, int[], List<UserInfo>) GetInGameInfo()
    {
        return (PlayerId, IsMafia, Mafias, Users);
    }
}