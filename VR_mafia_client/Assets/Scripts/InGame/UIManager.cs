﻿using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    private static UIManager instance;
    public static UIManager Instance
    {
        get
        {
            if (!instance)
            {
                instance = FindObjectOfType(typeof(UIManager)) as UIManager;
            }

            return instance;
        }
    }

    public GameObject killIcon;
    private Image killFillImage;
    private Image killCheckUI;
    [HideInInspector] public bool canKill;

    public Image deadReportImage;

    public void InitUI(bool isMafia)
    {
        if (isMafia)
        {
            Transform killIconTR = killIcon.transform;
            killFillImage = killIconTR.Find("Fill Image").GetComponent<Image>();
            killCheckUI = killIconTR.Find("Kill Check UI").GetComponent<Image>();
            canKill = true;
        }
        else
        {
            killIcon.SetActive(false);
        }
    }

    public void KillCheckUI(bool onKillTarget)
    {
        if (onKillTarget)
            killCheckUI.color = Color.red;
        else
            killCheckUI.color = Color.white;
    }
    public void UpdateKillUI()
    {
        StartCoroutine(UpdateCoolTime(killFillImage, 5f));
    }

    public void DeadReportUI(bool canDeadReport)
    {
        if (canDeadReport)
            deadReportImage.color = Color.red;
        else
            deadReportImage.color = Color.white;
    }

    IEnumerator UpdateCoolTime(Image fill, float coolTime)
    {
        float ct = coolTime;
        canKill = false;

        fill.fillAmount = 1f;
        while(0 < ct)
        {
            ct -= 0.5f;
            fill.fillAmount = ct / coolTime;

            yield return new WaitForSeconds(0.5f);
        }
        fill.fillAmount = 0f;

        canKill = true;
    }
}
