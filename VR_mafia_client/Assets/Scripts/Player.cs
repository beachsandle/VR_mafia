using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]

public class Player : MonoBehaviour
{
    private CharacterController CC;
    private Vector3 moveDirection = Vector3.zero;

    [Header("속도 및 중력")]
    public float moveSpeed = 4.0F;
    public float jumpSpeed = 8.0F;
    public float rotateSpeed = 2.0F;
    public float gravity = 20.0F;

    void Start()
    {
        CC = GetComponent<CharacterController>();
        if (!CC)
        {
            Debug.Log("CC is empty..!");
        }
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        Move();
        Rotate();
    }

    void Move()
    {
        if (CC.isGrounded)
        {
            moveDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
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
        transform.Rotate(0f, Input.GetAxis("Mouse X") * rotateSpeed, 0f);
    }
}
