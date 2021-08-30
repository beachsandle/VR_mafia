using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneLoader : MonoBehaviour
{
    #region singleton
    private static SceneLoader instance;
    public static SceneLoader Instance
    {
        get
        {
            if (instance == null)
                instance = FindObjectOfType<SceneLoader>();
            return instance;
        }
    }
    #endregion

    #region field
    [SerializeField] private float loadingTime = 1f;
    private Transform canvas;
    private Slider progressBar;
    #endregion

    #region unity message
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        canvas = transform.Find("Canvas");
        progressBar = canvas.Find("ProgressBar").GetComponentInChildren<Slider>();
        canvas.gameObject.SetActive(false);
    }
    #endregion

    #region method
    public void Load(string sceneName)
    {
        StartCoroutine(LoadScene(sceneName));
    }
    private IEnumerator LoadScene(string sceneName)
    {
        var ao = SceneManager.LoadSceneAsync(sceneName);
        progressBar.value = 0f;
        float timer = 0f;
        canvas.gameObject.SetActive(true);
        while (!ao.isDone || timer < loadingTime)
        {
            yield return null;
            timer += Time.deltaTime;
            progressBar.value = Mathf.Min(ao.progress, timer / loadingTime);
        }
        ao.allowSceneActivation = true;
        canvas.gameObject.SetActive(false);
    }
    #endregion
}
