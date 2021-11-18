using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using UnityEngine.UI;

public class FirstMission : MonoBehaviour
{
    const int DIGIT = 4;
    private int password;
    private int inputNum;
    private int buttonLeft = DIGIT;
    private Button[] buttons;

    public event Action<bool> MissionEnded;

    private void Awake()
    {
        FindReference();
        InitPassword();
        InitButtonEvent();
    }

    private void OnDisable()
    {
        for(int i = 0; i < DIGIT; i++)
        {
            buttons[i].interactable = true;
        }
    }

    private void FindReference()
    {
        buttons = GetComponentsInChildren<Button>();
    }

    private void InitButtonEvent()
    {
        for(int i = 0; i < DIGIT; i++)
        {
            int num = i + 1;
            buttons[i].onClick.AddListener(() =>
            {
                OnNumButton(num);
            });
        }
    }

    private void InitPassword()
    {
        List<int> list = Enumerable.Range(1, DIGIT).ToList();
        int place = 1000;
        while(0 < list.Count)
        {
            int idx = UnityEngine.Random.Range(0, list.Count - 1);
            password += (place * list[idx]);
            list.RemoveAt(idx);
            place /= 10;
        }

        Debug.Log("password : " + password);
    }

    private void OnNumButton(int n)
    {
        buttonLeft--;

        inputNum += (int)Mathf.Pow(10, buttonLeft) * n;
        buttons[n - 1].interactable = false;

        if(buttonLeft == 0)
        {
            MissionFinish();
        }
    }

    public void MissionStart()
    {
        inputNum = 0;
        buttonLeft = 4;
        gameObject.SetActive(true);
    }

    private void MissionFinish()
    {
        bool isSuccess = (password == inputNum);
        Debug.Log("Mission : " + (isSuccess ? "Clear" : "Fail"));

        gameObject.SetActive(false);
        MissionEnded?.Invoke(isSuccess);
    }
}
