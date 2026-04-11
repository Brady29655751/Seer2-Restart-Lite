using System;
using System.Linq;
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

    public int page => selectModel.page;
    public int cursor => (selectModel.cursor.Length > 0) ? selectModel.cursor[0] : -1;
    public int globalCursor => (selectModel.globalCursor.Length > 0) ? selectModel.globalCursor[0] : -1;

    public void SetHintboxConfirmCallback(Action<Pet> callback) {
        hintboxOriginalCallback = callback;
    }

    public Action GetHintboxConfirmCallback() {
        Action callback = () => { hintboxOriginalCallback?.Invoke(currentSelectedPet); };
        return callback;
    }
    
}
