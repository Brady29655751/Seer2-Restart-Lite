using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeNameController : Module
{
    [SerializeField] private ChangeNameModel changeNameModel;
    [SerializeField] private ChangeNameView changeNameView;

    public event Action onChangeNameEmptyEvent;
    public event Action<string> onChangeNameSuccessEvent, onChangeAchievementSuccessEvent;

    public void InitName(string nickname) {
        changeNameView.SetNameText(nickname);
    }

    public void InitAchievement(int value) {
        changeNameModel.SetAchievement(value);
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

    public void OnChangeAchievement(int value) {
        changeNameModel.OnAfterChangeAchievement(value);
        onChangeAchievementSuccessEvent?.Invoke((value == 0) ? string.Empty : changeNameModel.achievement);
    }
}
