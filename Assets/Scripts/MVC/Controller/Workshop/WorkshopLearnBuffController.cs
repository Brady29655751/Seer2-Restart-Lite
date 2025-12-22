using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WorkshopLearnBuffController : Module
{
    [SerializeField] private WorkshopLearnBuffModel buffModel;
    [SerializeField] private WorkshopLearnBuffView buffView;
    [SerializeField] private PageView pageView;

    private Action<BuffInfo> onDIYSuccessCallback;

    public override void Init() 
    {
        buffView.OnPreviewBuff(buffModel.currentBuffInfo);
        OnSetPage();
    }

    public void SetDIYSuccessCallback(Action<BuffInfo> callback) {
        onDIYSuccessCallback = callback;
    }

    public void SetStorage(List<BuffInfo> storage)
    {
        buffModel.SetStorage(storage);
        OnSetPage();
    }

    public void Search() {
        buffModel.Search();
        OnSetPage();
    }

    public void Select(int index) {
        buffModel.Select(index);
        buffView.OnPreviewBuff(buffModel.currentBuffInfo);
    }

    public void PrevPage()
    {
        buffModel.PrevPage();
        OnSetPage();
    }

    public void NextPage()
    {
        buffModel.NextPage();
        OnSetPage();
    }

    private void OnSetPage()
    {
        buffView.SetSelections(buffModel.selections, Select);
        pageView?.SetPage(buffModel.page, buffModel.lastPage);
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
