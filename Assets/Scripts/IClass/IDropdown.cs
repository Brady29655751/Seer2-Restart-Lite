using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IDropdown : IMonoBehaviour
{
    private Dropdown dropdown;
    public int value {
        get => dropdown.value;
        set => dropdown.value = value;
    } 

    public int optionCount => dropdown.options.Count;

    protected override void Awake()
    {
        base.Awake();
        dropdown = gameObject.GetComponent<Dropdown>();
    }

    public void SetDropdownOptions(List<string> options) {
        dropdown.ClearOptions();
        dropdown.AddOptions(options);
    }

}
