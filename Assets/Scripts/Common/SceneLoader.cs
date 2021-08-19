using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
    public static void ToIntroScene()
    {
        SceneManager.LoadScene("Intro");
    }
    public void ToLobbyScene()
    {
        SceneManager.LoadScene("Lobby");
    }

    public void LobbyToWaitingRoomScene()
    {
        SceneManager.LoadScene("WaitingRoom");
    }
}