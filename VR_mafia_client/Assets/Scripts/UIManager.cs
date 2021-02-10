using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    private static UIManager _instance;
    public static UIManager instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<UIManager>();
            }

            return _instance;
        }
    }

    public GameObject KillUI;

    void Start()
    {

    }

    //void Update()
    //{
        
    //}

    public void Kill()
    {
        Debug.Log("Killed..!");
    }
}
