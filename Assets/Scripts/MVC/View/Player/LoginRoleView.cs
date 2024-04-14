using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoginRoleView : Module
{
    [SerializeField] private RectTransform roleTransform;
    [SerializeField] private Text nameText;
    
    public void SetChosen(bool chosen) {
        if (chosen) {
            roleTransform.localScale *= 1.1f;
        } else {
            roleTransform.localScale /= 1.1f;
        }
    }

    public void SetName(string name) {
        nameText.text = name;
    }

}
