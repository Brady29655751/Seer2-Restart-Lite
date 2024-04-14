using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IDropdown : IMonoBehaviour
{
    private Dropdown dropdown;
    public int value => dropdown.value;

    protected override void Awake()
    {
        base.Awake();
        dropdown = gameObject.GetComponent<Dropdown>();
    }

}
