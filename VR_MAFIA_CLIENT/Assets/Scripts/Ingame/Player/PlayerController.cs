using Photon.Pun;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Animator))]

public class PlayerController : MonoBehaviour
{
    #region field
    //reference
    private CharacterController cc;
    private Animator animator;
    private Head head;
    private Hand hand;
    private Transform findAnchor;
    //variable
    private Vector3 moveDirection = Vector3.zero;
    private float rotX, rotY;
    private RaycastHit hit;
    private int layermask;
    private int missionMask;
    //inspector
    [Header("Status")]
    public float moveSpeed = 5.0F;
    public float jumpSpeed = 7.0F;
    public float rotateSpeed = 100.0F;
    public float gravity = 20.0F;
    public float range = 5.0F;
    #endregion

    #region property
    private GameManager gm => GameManager.Instance;
    private bool isVR => PhotonManager.Instance.isVR;
    #endregion

    #region unity message
    private void Awake()
    {
        cc = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        head = GetComponentInChildren<Head>();
        findAnchor = head.transform;
    }
    private void Start()
    {
        layermask = (1 << (int)Global.Layers.Player);
        missionMask = (1 << (int)Global.Layers.Mission);
        InactiveMyRay();
    }
    private void Update()
    {
        if (isVR ? OVRInput.GetDown(OVRInput.Button.Three) : Input.GetKeyDown(KeyCode.V))
            ToggleVoiceState();

        if (gm.PhaseChanging || gm.IsVoting)
            return;
        Move();
        if (gm.MenuOpened)
            return;
        if (!isVR) Rotate();

        FindTarget();
        if (isVR ? OVRInput.GetDown(OVRInput.Button.SecondaryIndexTrigger) : Input.GetKeyDown(KeyCode.Q))
        {
            gm.OnKillButton();
            gm.OnMissionButton(); // 임시
        }
        else if (isVR ? OVRInput.GetDown(OVRInput.Button.One) : Input.GetKeyDown(KeyCode.E))
            gm.OnDeadReportButton();

    }
    private void OnDestroy()
    {
        if (isVR) //TODO: VR일 때 cc 바로 파괴 불가함
            Destroy(GetComponent<OVRPlayerController>());
        Destroy(cc);
    }
    #endregion

    #region method

    #region private
    private void Move()
    {
        if (cc.isGrounded)
        {
            animator.SetBool("jump", false);
            moveDirection = new Vector3(Input.GetAxis("Horizontal") * 0.7f, 0f, Input.GetAxis("Vertical"));
            if (moveDirection != Vector3.zero)
            {
                animator.SetBool("run", true);
            }
            else
            {
                animator.SetBool("run", false);
            }
            moveDirection = transform.TransformDirection(moveDirection);
            moveDirection *= moveSpeed;

            if (isVR ? OVRInput.Get(OVRInput.Button.Two) : Input.GetButton("Jump"))
            {
                moveDirection.y = jumpSpeed;
                animator.SetBool("jump", true);
            }
        }
        moveDirection.y -= gravity * Time.deltaTime;
        cc.Move(moveDirection * Time.deltaTime);
    }
    private void Rotate()
    {
        rotX += Input.GetAxis("Mouse Y") * rotateSpeed * Time.deltaTime;
        rotY += Input.GetAxis("Mouse X") * rotateSpeed * Time.deltaTime;

        if (rotX <= -90f) rotX = -90f;
        else if (90f <= rotX) rotX = 90f;

        transform.eulerAngles = new Vector3(0f, rotY, 0);
        head.transform.eulerAngles = new Vector3(-rotX, rotY, 0f);
    }
    private void InactiveMyRay()
    {
        gameObject.layer = (int)Global.Layers.IgnoreRaycast;
        head.gameObject.layer = (int)Global.Layers.IgnoreRaycast;
    }
    private void FindTarget()
    {
        Physics.Raycast(findAnchor.position, findAnchor.forward, out hit, range, layermask);
        Debug.DrawRay(findAnchor.position, findAnchor.forward * range, Color.red);
        gm.OnFoundTarget(hit.transform?.GetComponent<PhotonView>().Owner);

        Physics.Raycast(findAnchor.position, findAnchor.forward, out hit, range, missionMask);
        gm.OnFoundMission(hit.transform != null);
    }
    private void ToggleVoiceState()
    {
        gm.OnVoiceKey();
    }
    #endregion

    #region public
    public void InitLocalCharacter(GameObject cam)
    {
        cam.transform.parent = isVR ? transform : head.transform;
        cam.transform.localPosition = Vector3.zero;

        if (isVR)
        {
            hand = GetComponentInChildren<Hand>();
            findAnchor = hand.transform;
        }
    }
    public void MoveTo(Vector3 pos)
    {
        cc.enabled = false;
        moveDirection = Vector3.zero;
        transform.position = pos;
        cc.enabled = true;
    }
    #endregion

    #endregion
}
