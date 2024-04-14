using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PetSelectHintboxController : Module
{
    [SerializeField] private PetSelectHintboxModel hintboxModel;
    [SerializeField] private PetSelectHintboxView hintboxView;
    [SerializeField] private HintboxController baseController;

    public void SetHintboxConfirmCallback(Action<Pet> callback) {
        hintboxModel.SetHintboxConfirmCallback(callback);
        baseController.SetOptionCallback(hintboxModel.GetHintboxConfirmCallback());
    }

    
}
