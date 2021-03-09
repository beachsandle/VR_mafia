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

    public void CreateRoom(string rName = "rName", string hName = "hName", int hCount = 0, int rId = -1)
    {
        GameObject room = Instantiate(roomObject);
        room.transform.SetParent(content.transform);

        room.GetComponent<Room>().SetRoomInfo(rName, hName, hCount, rId);
    }
}
