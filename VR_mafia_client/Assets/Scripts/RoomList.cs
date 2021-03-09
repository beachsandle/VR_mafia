using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class RoomList : MonoBehaviour
{
    [SerializeField]
    private GameObject roomObject;

    private GameObject content;
    void Start()
    {
        content = transform.GetChild(0).GetChild(0).gameObject;
    }

    public void CreateRoom()
    {
        GameObject room = Instantiate(roomObject);
        room.transform.SetParent(content.transform);

        room.GetComponent<Room>().SetRoomInfo("a", "b", 1);
    }
}
