using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostController : MonoBehaviour
{
    private int currTargetNum;

    void Start()
    {
        currTargetNum = GetComponent<PlayerCharacter>().Number;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            currTargetNum = InGameManager.Instance.NextCamera(currTargetNum);
        }
    }
}
