using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectNumController : Module
{
    [SerializeField] private SelectNumModel selectNumModel;
    [SerializeField] private SelectNumView selectNumView;
    [SerializeField] private PageView pageView;

    public event Action<int> onValueChangedEvent;

    public int GetInputValue() {
        return selectNumModel.inputValue;
    }

    public void OnValueChanged(string value) {
        selectNumModel.OnValueChanged(value);
        selectNumView.SetValue(selectNumModel.inputValue);
        pageView?.SetPage(selectNumModel.inputValue, selectNumModel.maxValue, selectNumModel.minValue);
        onValueChangedEvent?.Invoke(selectNumModel.inputValue);
    }

    public void SetMinValue(int minValue) {
        selectNumModel.SetMinValue(minValue);
        OnValueChanged(selectNumModel.inputValue.ToString());
    }

    public void SetMaxValue(int maxValue) {
        selectNumModel.SetMaxValue(maxValue);
        OnValueChanged(selectNumModel.inputValue.ToString());
    }

    public void SetIcon(Sprite icon) {
        selectNumView.SetIcon(icon);
    }

    public Action GetOptionCallback(Action<int> callback) {
        if (callback == null)
            return null;

        Action optionCallback = () => { callback?.Invoke(selectNumModel.inputValue); };
        return optionCallback;
    }

    public void OnPrevButtonClick() {
        OnValueChanged((selectNumModel.inputValue - 1).ToString());
    }

    public void OnNextButtonClick() {
        OnValueChanged((selectNumModel.inputValue + 1).ToString());
    }
}
