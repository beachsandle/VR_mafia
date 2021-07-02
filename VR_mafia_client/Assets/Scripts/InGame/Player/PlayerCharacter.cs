using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyPacket;

public class PlayerCharacter : MonoBehaviour
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

        //transform.Find("Head").GetComponent<MeshRenderer>().material.color = Global.colors[Number - 1];
        //transform.Find("Body").GetComponent<MeshRenderer>().material.color = Global.colors[Number - 1];
    }

    public void Dead(bool isMe)
    {
        IsAlive = false;
        GetComponent<Animator>().applyRootMotion = true;
        GetComponent<Animator>().Play("death2");

        if (isMe)
        {
            MakeGhost();
        }
        Destroy(GetComponent<CharacterController>());

        gameObject.AddComponent<BoxCollider>().isTrigger = true;
    }
    private void MakeGhost()
    {
        Destroy(GetComponent<PlayerController>());

        gameObject.AddComponent<GhostController>();
    }

    public void MakeEmpty()
    {
        Destroy(GetComponent<Animator>());

        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }
}
