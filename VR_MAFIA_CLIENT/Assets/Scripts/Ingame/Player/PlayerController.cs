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
    //variable
    private Vector3 moveDirection = Vector3.zero;
    private float rotX, rotY;
    private RaycastHit hit;
    private int layermask;
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
    #endregion

    #region unity message
    private void Awake()
    {
        cc = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        head = GetComponentInChildren<Head>();
    }
    private void Start()
    {
        layermask = (1 << (int)Global.Layers.Player);
        InactiveMyRay();
    }
    private void Update()
    {
        if (gm.PhaseChanging || gm.IsVoting)
            return;
        Move();
        if (gm.MenuOpened)
            return;
        Rotate();
        FindTarget();
        if (Input.GetKeyDown(KeyCode.Q))
            gm.OnKillButton();
        else if (Input.GetKeyDown(KeyCode.E))
            gm.OnDeadReportButton();
    }
    private void OnDestroy()
    {
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

            if (Input.GetButton("Jump"))
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
        Physics.Raycast(head.transform.position, head.transform.forward, out hit, range, layermask);
        Debug.DrawRay(head.transform.position, head.transform.forward * range, Color.red);
        gm.OnFoundTarget(hit.transform?.GetComponent<PhotonView>().Owner);
    }
    #endregion

    #region public
    public void InitLocalCharacter(GameObject cam)
    {
        cam.transform.parent = transform;
        cam.transform.localPosition = Vector3.zero;
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
