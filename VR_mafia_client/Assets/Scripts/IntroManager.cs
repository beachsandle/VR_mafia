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
    
    public void DisplayText(string s)
    {
        Text.text = s;
    }
}
