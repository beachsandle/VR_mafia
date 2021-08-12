using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerCharacter))]
[RequireComponent(typeof(Animator))]

public class PlayerController : MonoBehaviour
{
    public CharacterController CC;
    private PlayerCharacter myInfo;
    private Animator anim;
    private Transform HEAD;
    private Transform BODY;

    private Vector3 moveDirection = Vector3.zero;
    private float rotX, rotY;

    private RaycastHit hit;
    private int layermask;
    private bool onKillTarget = false;
    private bool canDeadReport = false;

    [Header("Status")]
    public float moveSpeed = 5.0F;
    public float jumpSpeed = 7.0F;
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

    private void Awake()
    {
        CC = GetComponent<CharacterController>();
        myInfo = GetComponent<PlayerCharacter>();
        anim = GetComponent<Animator>();

        HEAD = transform.Find("Helmet_LOD0");
        BODY = transform.Find("Space Explorer_LOD0");
    }
    void Start()
    {
        layermask = (1 << (int)Global.Layers.Player);
        InactiveMyRay();
        HideMyCharacter();
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

    void InactiveMyRay()
    {
        gameObject.layer = (int)Global.Layers.IgnoreRaycast;
        HEAD.gameObject.layer = (int)Global.Layers.IgnoreRaycast;
        BODY.gameObject.layer = (int)Global.Layers.IgnoreRaycast;
    }
    void HideMyCharacter()
    {
        transform.Find("Head_1_LOD0").GetComponent<SkinnedMeshRenderer>().enabled = false;
        HEAD.GetComponent<SkinnedMeshRenderer>().enabled = false;
        BODY.GetComponent<SkinnedMeshRenderer>().enabled = false;

        if (SceneLoader.Instance.isVR)
        {
            transform.Find("Space Explorer_Forearm_LOD0").GetComponent<SkinnedMeshRenderer>().enabled = false;
            transform.Find("Space Explorer_ShoulderPad_LOD0").GetComponent<SkinnedMeshRenderer>().enabled = false;
        }
    }

    #region 상호작용
    void FindTarget()
    {
        if (Physics.Raycast(HEAD.transform.position, HEAD.transform.forward, out hit, range, layermask))
        {
            Debug.DrawRay(HEAD.transform.position, HEAD.transform.forward * hit.distance, Color.red);

            if (myInfo.IsMafia) onKillTarget = true;
            if (!hit.transform.GetComponent<PlayerCharacter>().IsAlive) canDeadReport = true;
        }
        else
        {
            Debug.DrawRay(HEAD.transform.position, HEAD.transform.forward * range, Color.red);

            if (myInfo.IsMafia) onKillTarget = false;
            canDeadReport = false;
        }

        if (myInfo.IsMafia)
        {
            UIManager.Instance.KillCheckUI(onKillTarget);
        }
        UIManager.Instance.DeadReportUI(canDeadReport);
    }

    void Kill()
    {
        if (onKillTarget && UIManager.Instance.canKill)
        {
            int targetID = hit.transform.GetComponent<PlayerCharacter>().ID;
            InGameManager.Instance.EmitKillReq(targetID);
        }
    }

    void DeadReport()
    {
        if (canDeadReport)
        {
            int deadID = hit.transform.GetComponent<PlayerCharacter>().ID;
            InGameManager.Instance.EmitDeadReport(deadID);
        }
    }
    #endregion

    #region 움직임 관련
    void Move()
    {
        if (CC.isGrounded)
        {
            anim.SetBool("jump", false);
            moveDirection = new Vector3(Input.GetAxis("Horizontal") * 0.7f, 0f, Input.GetAxis("Vertical"));
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

            if (Input.GetButton("Jump"))
            {
                moveDirection.y = jumpSpeed;
                anim.SetBool("jump", true);
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
