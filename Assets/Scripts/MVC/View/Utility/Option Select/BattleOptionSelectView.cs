using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class BattleOptionSelectView : OptionSelectView
{
    [SerializeField] private IButton[] optionButtons = new IButton[5];

    public override void Select(int index)
    {
        base.Select(index);
    }

    public override void SetInteractable(int index, bool value)
    {
        base.SetInteractable(index, value);

        if (!index.IsInRange(0, optionButtons.Length))
            return;
            
        optionButtons[index].SetInteractable(value);
    }

    public void SetInteractableAll(bool interactable) {
        for (int i = 0; i < optionButtons.Length; i++) {
            SetInteractable(i, interactable);
        }
    }

}
