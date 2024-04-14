using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OptionSelectController : Module
{
    [SerializeField] private bool defaultSelect = true;
    [SerializeField] private OptionSelectModel optionModel;
    [SerializeField] private OptionSelectView optionView;

    public override void Init()
    {
        base.Init();
        if (defaultSelect) {
            Select(0);
        }
    }

    public void Select(int index) {
        optionModel.Select(index);
        optionView.Select(optionModel.cursor.FirstOrDefault());
    }

    public void SetInteractable(int index, bool value) {
        optionView.SetInteractable(index, value);
    }
}
