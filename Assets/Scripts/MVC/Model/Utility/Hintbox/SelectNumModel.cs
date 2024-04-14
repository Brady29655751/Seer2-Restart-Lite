using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectNumModel : Module
{
    public int inputValue { get; protected set; } = 1;
    public int maxValue { get; protected set; } = 1;
    public int minValue { get; protected set; } = 1;

    public void OnValueChanged(string value) {
        int parsedValue = 0;
        if (!int.TryParse(value, out parsedValue))
            return;

        inputValue = Mathf.Clamp(parsedValue, minValue, maxValue);
    }

    public void SetMinValue(int min) {
        minValue = min;
        inputValue = Mathf.Clamp(inputValue, minValue, maxValue);
    }

    public void SetMaxValue(int max) {
        maxValue = max;
        inputValue = Mathf.Clamp(inputValue, minValue, maxValue);
    }
}
