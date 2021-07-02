using UnityEngine;
using UnityEngine.UI;

public class Fixed : MonoBehaviour
{
    private CanvasScaler cs;
    public Vector2 setRes = new Vector2(1920, 1080);
    private float setRatio;
    private float deviceRatio = 0;
    private Camera lt, rb;
    private void Awake()
    {
        cs = GetComponent<CanvasScaler>();
        cs.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        cs.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        cs.referenceResolution = setRes;
        setRatio = setRes.x / setRes.y;
        lt = new GameObject("lt").AddComponent<Camera>();
        rb = new GameObject("rb").AddComponent<Camera>();
        InitCamera(lt);
        InitCamera(rb);
    }
    private void InitCamera(Camera cam)
    {
        cam.depth = 0;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Color.black;
    }
    private void Update()
    {
        if (deviceRatio != (float)Screen.width / Screen.height)
        {
            deviceRatio = (float)Screen.width / Screen.height;
            SetResolution(); //게임 해상도 고정
        }
    }

    /* 해상도 설정하는 함수 */
    public void SetResolution()
    {
        if (setRatio < deviceRatio) // 기기의 해상도 비가 더 큰 경우
        {
            float newWidth = setRatio / deviceRatio; // 새로운 너비
            float x = (1f - newWidth) / 2f;
            lt.rect = new Rect(0f, 0f, x, 1f);
            Camera.main.rect = new Rect(x, 0f, newWidth, 1f); // 새로운 Rect 적용
            rb.rect = new Rect(x + newWidth, 0f, 1f, 1f);
            cs.matchWidthOrHeight = 1;
        }
        else // 게임의 해상도 비가 더 큰 경우
        {
            float newHeight = deviceRatio / setRatio; // 새로운 높이
            float y = (1f - newHeight) / 2f;
            lt.rect = new Rect(0f, 0f, 1f, y);
            Camera.main.rect = new Rect(0f, y, 1f, newHeight); // 새로운 Rect 적용
            rb.rect = new Rect(0f, y + newHeight, 1f, 1f);
            cs.matchWidthOrHeight = 0;
        }
    }
}