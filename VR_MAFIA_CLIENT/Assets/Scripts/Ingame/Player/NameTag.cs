using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NameTag : MonoBehaviour
{
    Transform mainCam;
    public Text nameText;

    void Start()
    {
        mainCam = Camera.main.transform;
    }

    void Update()
    {
        transform.LookAt(transform.position + mainCam.rotation * Vector3.forward, mainCam.rotation * Vector3.up);
    }
}
