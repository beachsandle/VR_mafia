using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class PlayerCharacter : MonoBehaviour
{
    public Player Owner { get; private set; }
    private Animator animator;
    private void OnEnable()
    {
        animator = GetComponent<Animator>();
    }
    public void Die()
    {
        animator.applyRootMotion = true;
        animator.Play("death2");
        if (Owner == PhotonNetwork.LocalPlayer)
            Destroy(GetComponent<PlayerController>());
    }
    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        Owner = info.Sender;
        GameManager.Instance.OnSpwanPlayer(this);
    }
}
