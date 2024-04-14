using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class HintboxController : Module
{
    // components
    [SerializeField] protected HintboxModel hintboxModel;
    [SerializeField] protected HintboxView hintboxView;

    public void SetOptionNum(int num) {
        hintboxModel.SetOptionNum(num);
        hintboxView.SetOptionNum(num);
    }
    
    public void SetSize(int x, int y) {
        hintboxView.SetSize(x, y);
    }

    public void SetTitle(string text = "提示", int fontsize = 20, FontOption font = FontOption.Zongyi) {
        hintboxView.SetTitle(text, fontsize, font);
    }

    public void SetContent(string text, int fontsize, FontOption font) {
        hintboxView.SetContent(text, fontsize, font);
    }

    public void SetOptionCallback(Action callback, bool isConfirm = true) {
        if (callback == null)
            return;
        hintboxView.SetOptionCallback(hintboxModel.optionNum, callback, isConfirm);
    }

}
