using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Animations;

public class StartButton : IButton
{
    public Animator anim;

    public void SetAnimActive(bool active) {
        anim.SetBool("OnPointerEnter", active);
    }   

}
