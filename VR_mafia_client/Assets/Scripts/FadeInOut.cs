using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FadeInOut : MonoBehaviour
{
    private static FadeInOut _instance;
    public static FadeInOut instance
    {
        get
        {
            if (!_instance)
            {
                _instance = FindObjectOfType(typeof(FadeInOut)) as FadeInOut;
            }

            return _instance;
        }
    }

    private Image image;

    private Color orginColor;
    private Color srcColor;
    private Color destColor;
    private float fadeTime = 1f;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        image = GetComponent<Image>();

        orginColor = image.color;
        image.color = Color.clear;

        gameObject.SetActive(false);
    }

    public void FadeIn()
    {
        gameObject.SetActive(true);

        srcColor = orginColor;
        destColor = Color.clear;

        StartCoroutine(Fade());
    }
    public void FadeOut()
    {
        gameObject.SetActive(true);

        srcColor = Color.clear;
        destColor = orginColor;

        StartCoroutine(Fade());
    }

    IEnumerator Fade()
    {
        float time = 0;
        while (time < 1)
        {
            image.color = Color.Lerp(srcColor, destColor, time);
            time += Time.deltaTime / fadeTime;

            yield return new WaitForEndOfFrame();
        }

        gameObject.SetActive(false);
    }
}
