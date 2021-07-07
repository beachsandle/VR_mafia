using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingManager : MonoBehaviour
{
    [SerializeField] private Image barFillImage;
    private string nextScene;

    private void Awake()
    {
        barFillImage.fillAmount = 0;
    }

    void Start()
    {
        nextScene = SceneLoader.Instance.NextScene;

        StartCoroutine(LoadScene());
    }

    private void SetBarFill(float f)
    {
        barFillImage.fillAmount = Mathf.Lerp(barFillImage.fillAmount, f, 0.5f);
    }

    private IEnumerator LoadScene()
    {
        yield return null;

        AsyncOperation asyncOperation;
        asyncOperation = SceneManager.LoadSceneAsync(nextScene);
        asyncOperation.allowSceneActivation = false;

        while (!asyncOperation.isDone)
        {
            SetBarFill(asyncOperation.progress);

            if (0.9f <= asyncOperation.progress)
            {
                //if (asyncOperation.progress == 1.0f) // 메시지 필요하면 사용
                //{
                //    asyncOperation.allowSceneActivation = true;
                //}
                SetBarFill(1);

                asyncOperation.allowSceneActivation = true;
            }

            yield return null;
        }
    }
}
