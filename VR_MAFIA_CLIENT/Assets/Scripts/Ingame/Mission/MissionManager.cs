using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissionManager : MonoBehaviour
{
    public GameObject missionObject;

    void Start()
    {
        GenerateMission();
    }

    void GenerateMission()
    {
        missionObject.AddComponent<FirstMission>();
    }
}
