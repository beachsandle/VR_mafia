using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]

public class PlayerController : MonoBehaviour
{
    public CharacterController CC;
    private Player myInfo;
    private Transform HEAD;
    private Transform BODY;
    private Vector3 moveDirection = Vector3.zero;
    private float rotX, rotY;

    private RaycastHit hit;
    private int layermask;
    private bool canKill = false;
    private bool canDeadReport = false;

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
        myInfo = GetComponent<Player>();

        HEAD = transform.GetChild(0);
        BODY = transform.GetChild(1);

        layermask = (1 << 10); // Layer : player
    }

    void Update()
    {
        if (InGameManager.Instance.phaseChange || InGameManager.Instance.isVoting) return;

        Move();
        Rotate();
        InGameManager.Instance.EmitMoveReq(transform.position, transform.rotation);

        if (InGameManager.Instance.menuState) return;

        FindTarget();

        if (Input.GetKeyDown(KeyCode.Q))
        {
            Kill();
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            DeadReport();
        }
    }


    #region 상호작용
    void FindTarget()
    {
        if (Physics.Raycast(transform.position, transform.forward, out hit, range, layermask))
        {
            Debug.DrawRay(transform.position, transform.forward * hit.distance, Color.red);

            if(myInfo.IsMafia) canKill = true;
            if(!hit.transform.GetComponent<Player>().IsAlive) canDeadReport = true;
        }
        else
        {
            Debug.DrawRay(transform.position, transform.forward * range, Color.red);

            if(myInfo.IsMafia) canKill = false;
            canDeadReport = false;
        }

        UIManager.instance.CanKill(canKill);
        UIManager.instance.CanDeadReport(canDeadReport);
    }

    void Kill()
    {
        if (canKill)
        {
            int targetID = hit.transform.GetComponent<Player>().ID;
            InGameManager.Instance.EmitKillReq(targetID);
        }
    }
    void DeadReport()
    {
        if (canDeadReport)
        {
            int deadID = hit.transform.GetComponent<Player>().ID;
            InGameManager.Instance.EmitDeadReport(deadID);
        }
    }
    #endregion

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
        if (InGameManager.Instance.menuState) return;

        rotX += Input.GetAxis("Mouse Y") * rotateSpeed * Time.deltaTime;
        rotY += Input.GetAxis("Mouse X") * rotateSpeed * Time.deltaTime;

        if (rotX <= -90f) rotX = -90f;
        else if (90f <= rotX) rotX = 90f;
        //if (90f <= rotY) rotY = 90f;
        //if (rotY <= -90f) rotY = -90f;

        transform.eulerAngles = new Vector3(0f, rotY, 0);
        HEAD.transform.eulerAngles = new Vector3(-rotX, rotY, 0f);
    }

    public void MoveTo(Vector3 pos)
    {
        CC.enabled = false;
        moveDirection = Vector3.zero;
        transform.position = pos;
        CC.enabled = true;
        InGameManager.Instance.EmitMoveReq(transform.position, transform.rotation);
    }
    #endregion
}
