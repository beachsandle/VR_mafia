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

    public GameObject killIcon;
    private Image killFillImage;
    private Image killCheckUI;

    public Image deadReportImage;

    public void InitUI(bool isMafia)
    {
        if (isMafia)
        {
            Transform killIconTR = killIcon.transform;
            killFillImage = killIconTR.Find("Fill Image").GetComponent<Image>();
            killCheckUI = killIconTR.Find("Kill Check UI").GetComponent<Image>();
        }
        else
        {
            killIcon.SetActive(false);
        }
    }

    public void KillCheckUI(bool canKill)
    {
        if (canKill)
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

        fill.fillAmount = 1f;
        while(0 < ct)
        {
            ct -= 0.5f;
            fill.fillAmount = ct / coolTime;

            yield return new WaitForSeconds(0.5f);
        }
        fill.fillAmount = 0f;
    }
}
