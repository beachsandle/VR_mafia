using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissionManager : MonoBehaviour
{
    private GameManager gm => GameManager.Instance;

    public GameObject missionObject;

    private Transform[] lockPositions;

    private void Awake()
    {
        lockPositions = transform.Find("LockPositions").GetComponentsInChildren<Transform>();
    }

    void Start()
    {
        GenerateMission();
    }

    void GenerateMission()
    {
        Vector3 pos = lockPositions[Random.Range(1, lockPositions.Length)].position;
        Instantiate(missionObject, pos, Quaternion.identity);
    }
}
