using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyPacket;

public class Player : MonoBehaviour
{
    public int Number { get; private set; }
    public int ID { get; private set; }
    public string Name { get; private set; }
    public bool IsAlive { get; private set; }
    public bool IsMafia { get; set; }
    public void InitPlayerInfo(int index, UserInfo info)
    {
        Number = index + 1;
        ID = info.Id;
        Name = info.Name;
        IsAlive = true;
        IsMafia = false;

        transform.Find("Head").GetComponent<MeshRenderer>().material.color = Global.colors[Number - 1];
        transform.Find("Body").GetComponent<MeshRenderer>().material.color = Global.colors[Number - 1];
    }

    public void Dead()
    {
        IsAlive = false;
        transform.Find("Body").gameObject.SetActive(false);
    }
}
