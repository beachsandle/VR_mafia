using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissionManager : MonoBehaviour
{
    private GameManager gm => GameManager.Instance;

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
