using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VotingPanel : MonoBehaviour
{
    private void OnDisable()
    {
        var votingContent = transform.GetChild(0);

        for (int i = 0; i < votingContent.childCount; i++)
        {
            var content = votingContent.GetChild(i);
            if (content.GetComponent<Button>())
            {
                content.GetComponent<Button>().image.color = Color.white;
                content.Find("Count Text").GetComponent<Text>().text = "0";
            }
        }
    }
}
