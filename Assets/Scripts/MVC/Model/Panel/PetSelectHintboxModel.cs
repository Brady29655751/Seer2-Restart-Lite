using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PetSelectHintboxModel : Module
{
    [SerializeField] private PetSelectModel selectModel;

    private Action<Pet> hintboxOriginalCallback;
    public Action hintboxConfirmCallback => GetHintboxConfirmCallback();
    public Pet currentSelectedPet => (selectModel.currentSelectedItems.Length > 0) ? 
        selectModel.currentSelectedItems[0] : null;

    public void SetHintboxConfirmCallback(Action<Pet> callback) {
        hintboxOriginalCallback = callback;
    }

    public Action GetHintboxConfirmCallback() {
        Action callback = () => { hintboxOriginalCallback?.Invoke(currentSelectedPet); };
        return callback;
    }
    
}
