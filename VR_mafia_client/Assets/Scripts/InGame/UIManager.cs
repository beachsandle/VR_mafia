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

    public GameObject killUI;
    public GameObject deadReportUI;

    void Start()
    {

    }

    public void InitUI(bool isMafia)
    {
        if (!isMafia)
        {
            killUI.SetActive(false);
        }
    }

    public void CanKill(bool canKill)
    {
        if (canKill)
            killUI.GetComponent<Image>().color = Color.red;
        else
            killUI.GetComponent<Image>().color = Color.white;
    }

    public void CanDeadReport(bool canDeadReport)
    {
        if (canDeadReport)
            deadReportUI.GetComponent<Image>().color = Color.red;
        else
            deadReportUI.GetComponent<Image>().color = Color.white;
    }
}
