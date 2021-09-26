using Photon.Pun;
using UnityEngine;

public class GhostController : MonoBehaviour
{
    #region field
    //reference
    private CharacterController cc;
    private Head head;
    //variable
    private Vector3 moveDirection = Vector3.zero;
    private float rotX, rotY;
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
        cc.enabled = false;
        head = GetComponentInChildren<Head>();
    }
    private void Update()
    {
        if (gm.PhaseChanging || gm.IsVoting)
            return;
        Move();
        if (gm.MenuOpened)
            return;
        if (!isVR) Rotate();
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
            moveDirection = new Vector3(Input.GetAxis("Horizontal") * 0.7f, 0f, Input.GetAxis("Vertical"));
            moveDirection = transform.TransformDirection(moveDirection);
            moveDirection *= moveSpeed;

            if (isVR ? OVRInput.Get(OVRInput.Button.Two) : Input.GetButton("Jump"))
            {
                moveDirection.y = jumpSpeed;
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
    #endregion

    #region public
    public void InitGhost(GameObject cam)
    {
        cam.transform.parent = isVR ? transform : head.transform;
        cam.transform.localPosition = Vector3.zero;
        if (isVR)
            cam.transform.localPosition = new Vector3(0, 2f, 0);
        cc.enabled = true;
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
