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
    public static void ReturnToIntroScene()
    {
        SceneManager.LoadScene("Intro");
    }
    public void IntroToLobbyScene()
    {
        SceneManager.LoadScene("Lobby");
    }
}