using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Photon.Realtime;

public class Room : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Text roomName;
    [SerializeField] private Text hostName;
    [SerializeField] private Text headCount;

    private float last = 0f;
    private readonly float interval = 0.4f;
    private RoomInfo roomInfo;

    public Action<RoomInfo> Clicked;
    public Action DoubleClicked;

    public void OnPointerClick(PointerEventData pointerEventData)
    {
        if (Time.time - last < interval)
            DoubleClicked?.Invoke();
        else
            Clicked?.Invoke(roomInfo);
        last = Time.time;
    }

    public void SetRoomInfo(RoomInfo roomInfo)
    {
        this.roomInfo = roomInfo;
        roomName.text = roomInfo.Name;
        hostName.text = roomInfo.HostName();
        headCount.text = $"{roomInfo.PlayerCount}/{roomInfo.MaxPlayers}";
    }
}
