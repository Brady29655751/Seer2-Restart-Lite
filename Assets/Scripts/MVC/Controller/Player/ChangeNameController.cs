using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeNameController : Module
{
    [SerializeField] private ChangeNameModel changeNameModel;
    [SerializeField] private ChangeNameView changeNameView;

    public event Action onChangeNameEmptyEvent;
    public event Action<string> onChangeNameSuccessEvent;

    public void InitName(string nickname) {
        changeNameView.SetNameText(nickname);
    }

    public void OnChangeName() {
        if (!changeNameModel.isDone) {
            string inputString = changeNameModel.inputString;
            if (string.IsNullOrEmpty(inputString)) {
                onChangeNameEmptyEvent?.Invoke();
                return;
            } 
            changeNameView.SetNameText(inputString);
            onChangeNameSuccessEvent?.Invoke(inputString);
        }
        changeNameModel.OnAfterChangeName();
        changeNameView.OnAfterChangeName(changeNameModel.isDone);
    }
}
