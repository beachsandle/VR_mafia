using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Room : MonoBehaviour
{
    [SerializeField] private Text roomName;
    [SerializeField] private Text hostName;
    [SerializeField] private Text headCount;
    private int roomId = -1;

    public void SetRoomInfo(string rName, string hName, int hCount, int rId)
    {
        roomName.text = rName;
        hostName.text = hName;
        headCount.text = hCount.ToString() + "/8";
        roomId = rId;
    }
}
