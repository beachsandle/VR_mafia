using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class PlayerCharacter : MonoBehaviour, IPunInstantiateMagicCallback
{
    public Player Owner { get; private set; }
    private Animator animator;
    private NameTag nameTag;
    public PlayerController Controller { get; private set; } = null;


    private void OnEnable()
    {
        animator = GetComponent<Animator>();
        nameTag = GetComponentInChildren<NameTag>();
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

        nameTag.nameText.text = Owner.NickName;
    }
    public void Hide()
    {
        foreach (var render in GetComponentsInChildren<Renderer>(false))
            render.enabled = false;

        nameTag.gameObject.SetActive(false);
    }

    public void InitLocalCharacter(GameObject cameraObj, bool isVR)
    {
        Controller = gameObject.AddComponent<PlayerController>();
        Controller.InitLocalCharacter(cameraObj);
        if (isVR)
        {
            OVRPlayerController ovrPlayerController = gameObject.AddComponent<OVRPlayerController>();
            ovrPlayerController.BackAndSideDampen = 1f;
            ovrPlayerController.SnapRotation = false;
            ovrPlayerController.RotateAroundGuardianCenter = true;
            ovrPlayerController.GravityModifier = 0.0f;
        }
    }
}
