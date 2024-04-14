using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PetSelectHintbox : Hintbox
{
    [SerializeField] private PetSelectHintboxController panelController;
    [SerializeField] private PetSelectController selectController;

    public override void Init()
    {
        base.Init();
    }

    public void SetStorage(List<Pet> storage) {
        selectController.SetStorage(storage);
        selectController.Select(0);
    }

    public void SetConfirmSelectCallback(Action<Pet> callback) {
        panelController.SetHintboxConfirmCallback(callback);
    }


}
