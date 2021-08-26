using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class PlayerCharacter : MonoBehaviour, IPunInstantiateMagicCallback
{
    public Player Owner { get; private set; }
    private Animator animator;
    public PlayerController Controller { get; private set; } = null;
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
        if (Owner.IsLocal)
        {
            Controller = gameObject.AddComponent<PlayerController>();
            Controller.SetCamera(Camera.main);
            Hide();
        }
    }
    public void Hide()
    {
        foreach (var render in GetComponentsInChildren<Renderer>(false))
            render.enabled = false;
    }
}
