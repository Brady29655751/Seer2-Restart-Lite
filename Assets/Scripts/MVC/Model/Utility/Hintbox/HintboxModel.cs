using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HintboxModel : Module
{
    [SerializeField] public bool isWithBackground = false;
    [SerializeField] public bool shouldDestroyWhenClose = true;
    [SerializeField] public int optionNum = 1;

    public void SetOptionNum(int num) {
        optionNum = num;
    }

}
