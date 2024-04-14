using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(InputField))]
public class IInputField : IMonoBehaviour
{
    public string inputString => inputField?.text;
    public InputField inputField { get; protected set; } = null;
    public Text placeHolderText = null;

    
    protected override void Awake() {
        base.Awake();
        inputField = gameObject.GetComponent<InputField>();
    }

    public virtual void OnValueChanged(string value) {

    }

    public virtual void OnEndEdit(string value) {

    }

    public virtual void SetInputString(string value) {
        if (inputField == null)
            return;
            
        inputField.text = value;
    }

    public virtual void SetPlaceHolderText(string value) {
        placeHolderText?.SetText(value);
    }
}
