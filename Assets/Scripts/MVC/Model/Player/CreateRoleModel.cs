using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateRoleModel : Module
{
    [SerializeField] private IInputField ipf;
    public bool gender { get; private set; } = false;
    public string inputString => ipf.inputString;

    public void SetGender(bool gender) {
        this.gender = gender;
    }
}
