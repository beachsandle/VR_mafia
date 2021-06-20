using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoMove : MonoBehaviour
{
    private CharacterController CC;
    private Vector3 moveDirection = Vector3.zero;
    private Vector3 targetPos = Vector3.zero;
    private Animator anim;

    [Header("Status")]
    public float moveSpeed = 6.0F;
    public float jumpSpeed = 8.0F;
    public float rotateSpeed = 100.0F;
    public float gravity = 20.0F;
    public float range = 5.0F;

    void Start()
    {
        CC = GetComponent<CharacterController>();
        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (CC.isGrounded)
        {
            anim.SetBool("jump", false);
            moveDirection = new Vector3(targetPos.x - transform.position.x, 0f, targetPos.z - transform.position.z);
            if (moveDirection != Vector3.zero)
            {
                anim.SetBool("run", true);
            }
            else
            {
                anim.SetBool("run", false);
            }
            moveDirection = transform.TransformDirection(moveDirection);
            moveDirection *= moveSpeed;

            //if (Input.GetButton("Jump"))
            //{
            //    moveDirection.y = jumpSpeed;
            //    anim.SetBool("jump", true);
            //}
        }
        else
        {
            anim.SetBool("jump", true);
        }
        moveDirection.y -= gravity * Time.deltaTime;
        CC.Move(moveDirection * Time.deltaTime);
    }

    public void SetTargetPos(Vector3 pos)
    {
        targetPos = pos;
    }
}
