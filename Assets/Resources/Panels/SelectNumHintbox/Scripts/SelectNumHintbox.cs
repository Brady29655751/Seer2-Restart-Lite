using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectNumHintbox : Hintbox
{
    [SerializeField] private SelectNumController selectNumController;

    public int GetInputValue() {
        return selectNumController.GetInputValue();
    }

    public void OnValueChanged(string value) {
        selectNumController.OnValueChanged(value);
    }

    public void SetMinValue(int minValue) {
        selectNumController.SetMinValue(minValue);
    }

    public void SetMaxValue(int maxValue) {
        selectNumController.SetMaxValue(maxValue);
    }

    public void SetIcon(Sprite icon) {
        selectNumController.SetIcon(icon);
    }

    public void OnPrevButtonClick() {
        selectNumController.OnPrevButtonClick();
    }

    public void OnNextButtonClick() {
        selectNumController.OnNextButtonClick();
    }
}
