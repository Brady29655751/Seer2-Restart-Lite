using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeNameModel : Module
{
    [SerializeField] private IInputField ipf;
    public bool isDone { get; private set; } = true;
    public string inputString => ipf.inputString;

    public void SetDone(bool done) {
        isDone = done;
    }

    public void OnAfterChangeName() {
        SetDone(!isDone);
    }

}
