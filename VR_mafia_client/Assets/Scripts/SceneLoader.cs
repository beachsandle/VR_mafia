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
    public string NextScene { get; private set; }

    public bool isVR;

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

        string ends = isVR ? "VR" : "";
        PhotonManager.Instance.OnConnect += () =>
        {
            SceneManager.LoadScene("Lobby" + ends);
        };
        PhotonManager.Instance.OnJoined += () =>
        {
            NextScene = "WaitingRoom" + ends;
            SceneManager.LoadScene("Loading" + ends);
        };
        PhotonManager.Instance.OnLeft += () =>
        {
            SceneManager.LoadScene("Lobby" + ends);
        };
    }
    public void IntroToLobby(int pid)
    {
        PlayerId = pid;
        UserName = "Player" + PlayerId;
        PhotonManager.Instance.Connect(pid);
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
    }
    public (string, List<UserInfo>) GetWaitingRoomInfo()
    {
        return (RoomName, Users);
    }
    public void WaitingRoomToLobby()
    {
        Users.Clear();
        PhotonManager.Instance.LeaveRoom();
    }
    public void WaitingRoomToInGame(bool isMafia, int[] mafias, List<UserInfo> users)
    {
        IsMafia = isMafia;
        Users = users;
        Mafias = mafias;

        string ends = isVR ? "VR" : "";
        NextScene = "InGame" + ends;
        SceneManager.LoadScene("Loading" + ends);
    }
    public (int, bool, int[], List<UserInfo>) GetInGameInfo()
    {
        return (PlayerId, IsMafia, Mafias, Users);
    }
    public void InGameToLobby()
    {
        SceneManager.LoadScene("Lobby");
    }
}