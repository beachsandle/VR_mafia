using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Room : MonoBehaviour
{
    [SerializeField] private Text roomName;
    [SerializeField] private Text hostName;
    [SerializeField] private Text headCount;

    public void SetRoomInfo(string rName, string hName, int hCount)
    {
        roomName.text = rName;
        hostName.text = hName;
        headCount.text = hCount.ToString() + "/8";
    }
}
