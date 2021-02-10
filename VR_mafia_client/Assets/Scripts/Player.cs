using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]

public class Player : MonoBehaviour
{
    private CharacterController CC;
    private Transform HEAD;
    private Transform BODY;
    private Vector3 moveDirection = Vector3.zero;

    [Header("속도 및 중력")]
    public float moveSpeed = 4.0F;
    public float jumpSpeed = 8.0F;
    public float rotateSpeed = 100.0F;
    public float gravity = 20.0F;

    private float rotX, rotY;

    void Start()
    {
        CC = GetComponent<CharacterController>();
        if (!CC)
        {
            Debug.Log("CC is empty..!");
        }
        HEAD = transform.GetChild(0);
        BODY = transform.GetChild(1);

        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        Rotate();
        Move();
    }

    #region 움직임 관련
    void Move()
    {
        if (CC.isGrounded)
        {
            moveDirection = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));
            moveDirection = transform.TransformDirection(moveDirection);
            moveDirection *= moveSpeed;
            if (Input.GetButton("Jump"))
            {
                moveDirection.y = jumpSpeed;
            }
        }
        moveDirection.y -= gravity * Time.deltaTime;
        CC.Move(moveDirection * Time.deltaTime);
    }
    void Rotate()
    {
        rotX += Input.GetAxis("Mouse Y") * rotateSpeed * Time.deltaTime;
        rotY += Input.GetAxis("Mouse X") * rotateSpeed * Time.deltaTime;

        if (rotX <= -90f) rotX = -90f;
        else if (90f <= rotX) rotX = 90f;
        //if (90f <= rotY) rotY = 90f;
        //if (rotY <= -90f) rotY = -90f;

        transform.eulerAngles = new Vector3(0f, rotY, 0);
        HEAD.transform.eulerAngles = new Vector3(-rotX, rotY, 0f);
    }
    #endregion
}
