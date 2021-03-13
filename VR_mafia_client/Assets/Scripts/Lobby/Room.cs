using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Room : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Text roomName;
    [SerializeField] private Text hostName;
    [SerializeField] private Text headCount;

    private int roomId = -1;
    private float last = 0f;
    private float interval = 0.4f;

    public void OnPointerClick(PointerEventData pointerEventData)
    {
        if (Time.time - last < interval)
            Debug.Log(name + " DoubleClicked!");

        last = Time.time;
    }

    public void SetRoomInfo(string rName, string hName, int hCount, int rId)
    {
        roomName.text = rName;
        hostName.text = hName;
        headCount.text = hCount.ToString() + "/8";
        roomId = rId;
    }
}
