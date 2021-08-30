using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class RoomList : MonoBehaviour
{
    [SerializeField] private Room roomPrefab;
    private GameObject content;
    private List<Room> rooms;

    public Action<RoomInfo> RoomClicked;
    public Action RoomDoubleClicked;

    void Awake()
    {
        content = transform.Find("Viewport").Find("Content").gameObject;
        rooms = new List<Room>();
    }

    private void OnRoomClicked(RoomInfo info) => RoomClicked?.Invoke(info);
    private void OnRoomDoubleClicked() => RoomDoubleClicked?.Invoke();

    public void Clear()
    {
        foreach (var r in rooms)
            Destroy(r.gameObject);
        rooms.Clear();
    }
    public void CreateRoom(RoomInfo roomInfo)
    {
        if (roomInfo.PlayerCount == 0 || !roomInfo.IsOpen)
            return;
        var room = Instantiate(roomPrefab, content.transform);
        room.SetRoomInfo(roomInfo);
        room.Clicked += OnRoomClicked;
        room.DoubleClicked += OnRoomDoubleClicked;
        rooms.Add(room);
    }
    public void CreateRooms(List<RoomInfo> roomInfos)
    {
        foreach (var info in roomInfos)
            CreateRoom(info);
    }
}
