using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class RoomList : MonoBehaviour
{
    [SerializeField]
    private GameObject roomObject;

    private GameObject content;


    public int select;

    void Start()
    {
        content = transform.GetChild(0).GetChild(0).gameObject;
    }
    public void Clear()
    {
        for (int i = 0, end = content.transform.childCount; i < end; ++i)
        {
            Destroy(content.transform.GetChild(i).gameObject);
        }
    }
    public void CreateRoom(string rName = "rName", string hName = "hName", int hCount = 0, int rId = -1)
    {
        GameObject room = Instantiate(roomObject, content.transform);
        room.name = string.Format("room({0})", rId);

        room.GetComponent<Room>().SetRoomInfo(rName, hName, hCount, rId);
    }
}
