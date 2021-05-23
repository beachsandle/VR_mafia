using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyPacket;

public class Player : MonoBehaviour
{
    public bool IsAlive { get; private set; }
    public int Number { get; private set; }
    public string Name { get; private set; }

    public void InitPlayerInfo(int index, UserInfo info)
    {
        IsAlive = true;
        Number = index;
        Name = info.Name;
    }

    public void Dead()
    {
        IsAlive = false;
        transform.Find("Body").gameObject.SetActive(false);
    }
}
