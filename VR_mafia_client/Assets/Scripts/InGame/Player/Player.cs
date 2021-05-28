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

    public void InitPlayerInfo(int index, UserInfo info)
    {
        Number = index;

        ID = info.Id;
        Name = info.Name;
        IsAlive = true;
    }

    public void Dead()
    {
        IsAlive = false;
        transform.Find("Body").gameObject.SetActive(false);
    }
}
