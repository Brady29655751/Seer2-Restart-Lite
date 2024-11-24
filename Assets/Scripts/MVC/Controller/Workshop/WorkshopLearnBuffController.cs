using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorkshopLearnBuffController : Module
{
    [SerializeField] private WorkshopLearnBuffModel buffModel;
    [SerializeField] private WorkshopLearnBuffView buffView;

    private Action<BuffInfo> onDIYSuccessCallback;
    public override void Init() {
        buffView.OnPreviewBuff(buffModel.currentBuffInfo);
    }

    public void SetDIYSuccessCallback(Action<BuffInfo> callback) {
        onDIYSuccessCallback = callback;
    }

    public void Search() {
        buffModel.Search();
        buffView.SetSelections(buffModel.buffInfoList, Select);
    }

    public void Select(int index) {
        buffModel.Select(index);
        buffView.OnPreviewBuff(buffModel.currentBuffInfo);
    }

    public void OnCancelLearnBuff() {

    }

    public void OnDIYLearnBuff() {
        if (!VerifyDIYLearnBuff(out var error)) {
            Hintbox.OpenHintboxWithContent(error, 16);
            return;
        }
        OnConfirmDIYLearnBuff();
    }

    private bool VerifyDIYLearnBuff(out string error) {
        return buffModel.VerifyDIYLearnBuff(out error);
    }

    public void OnConfirmDIYLearnBuff() {
        onDIYSuccessCallback?.Invoke(buffModel.currentBuffInfo);
    }
}
