using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OptionSelectModel : SelectModel<GameObject>
{
    protected override void Awake()
    {
        base.Awake();
        for (int i = 0; i < selections.Length; i++) {
            selections[i]?.SetActive(true);
            selections[i]?.SetActive(false);
        }
    }

    public override void Select(int index)
    {
        base.Select(index);
        for (int i = 0; i < selections.Length; i++) {
            selections[i]?.SetActive(i == index);
        }
    }
}
