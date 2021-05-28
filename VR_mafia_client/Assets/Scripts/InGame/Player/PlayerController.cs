using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]

public class PlayerController : MonoBehaviour
{
    public CharacterController CC;
    private Transform HEAD;
    private Transform BODY;
    private Vector3 moveDirection = Vector3.zero;
    private float rotX, rotY;

    private RaycastHit hit;
    private int layermask;

    [Header("Status")]
    public float moveSpeed = 4.0F;
    public float jumpSpeed = 8.0F;
    public float rotateSpeed = 100.0F;
    public float gravity = 20.0F;
    public float range = 5.0F;

    public bool ControllerEnabled
    {
        get
        {
            return CC.enabled;
        }
        set
        {
            CC.enabled = value;
            moveDirection = Vector3.zero;
        }
    }

    void Start()
    {
        CC = GetComponent<CharacterController>();
        if (!CC)
        {
            Debug.Log("CC is empty..!");
        }

        HEAD = transform.GetChild(0);
        BODY = transform.GetChild(1);

        layermask = (1 << 10);
    }

    void Update()
    {
        if (InGameManager.instance.phaseChange || InGameManager.instance.isVoting) return;

        Move();
        Rotate();
        ClientManager.instance.EmitMove(transform.position, transform.rotation);

        if (InGameManager.instance.menuState) return;

        FindTarget();

        if (Input.GetKeyDown(KeyCode.Q))
        {
            Kill();
        }
    }

    void FindTarget()
    {
        if (Physics.Raycast(transform.position, transform.forward, out hit, range, layermask))
        {
            Debug.DrawRay(transform.position, transform.forward * hit.distance, Color.red);

            UIManager.instance.CanKill(true);
        }
        else
        {
            Debug.DrawRay(transform.position, transform.forward * range, Color.red);

            UIManager.instance.CanKill(false);
        }
    }

    void Kill()
    {
        if (hit.transform)
        {
            int targetID = int.Parse(hit.transform.name.Split('_')[1]);

            ClientManager.instance.EmitKillReq(targetID);
        }
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
        if (InGameManager.instance.menuState) return;

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
