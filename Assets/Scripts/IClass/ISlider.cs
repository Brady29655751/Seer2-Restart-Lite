using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ISlider : IMonoBehaviour
{
    [SerializeField] private Text titleText;
    [SerializeField] private Slider slider;
    [SerializeField] private Text valueText;

    public void SetSliderValue(float value) {
        slider.value = value;
    }

    public void OnValueChanged(float value) {
        if (valueText == null)
            return;

        valueText.text = ((int)value).ToString();
    }
}
