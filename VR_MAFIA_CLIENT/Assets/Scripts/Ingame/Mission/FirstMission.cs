using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FirstMission : MonoBehaviour
{
    private int num = 4;
    private int password;

    void Start()
    {
        InitPassword();
    }

    private void InitPassword()
    {
        List<int> list = Enumerable.Range(1, num).ToList();
        int place = 1000;
        while(0 < list.Count)
        {
            int idx = Random.Range(0, list.Count - 1);
            password += (place * list[idx]);
            list.RemoveAt(idx);
            place /= 10;
        }
    }
}
