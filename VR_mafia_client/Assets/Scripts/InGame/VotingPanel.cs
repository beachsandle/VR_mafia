using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VotingPanel : MonoBehaviour
{
    private void OnDisable()
    {
        var votingContent = transform.GetChild(0);

        foreach (var btn in votingContent.GetComponentsInChildren<Button>())
        {
            btn.transform.Find("Text").GetComponent<Text>().color = Color.white;
            btn.transform.Find("Count Text").GetComponent<Text>().text = "0";
        }
    }
}
