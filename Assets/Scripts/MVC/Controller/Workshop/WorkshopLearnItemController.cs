using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorkshopLearnItemController : Module
{
    [SerializeField] private WorkshopLearnItemModel itemModel;
    [SerializeField] private WorkshopLearnItemView itemView;

    private Action<ItemInfo> onDIYSuccessCallback;

    public override void Init() {
        itemView.OnPreviewItem(itemModel.currentItemInfo);
    }

    public void SetDIYSuccessCallback(Action<ItemInfo> callback) {
        onDIYSuccessCallback = callback;
    }

    public void Search() {
        itemModel.Search();
        itemView.SetSelections(itemModel.itemInfoList, Select);
    }

    public void Select(int index) {
        itemModel.Select(index);
        itemView.OnPreviewItem(itemModel.currentItemInfo);
    }

    public void OnCancelLearnItem() {

    }

    public void OnDIYLearnItem() {
        if (!VerifyDIYLearnItem(out var error)) {
            Hintbox.OpenHintboxWithContent(error, 16);
            return;
        }
        OnConfirmDIYLearnItem();
    }

    private bool VerifyDIYLearnItem(out string error) {
        return itemModel.VerifyDIYLearnItem(out error);
    }

    private void OnConfirmDIYLearnItem() {
        onDIYSuccessCallback?.Invoke(itemModel.currentItemInfo);
    }

    public void SetInfoPromptActive(bool active) {
        itemView.SetInfoPromptActive(active);
    }
    
    public void ShowItemInfo() {
        itemView.ShowItemInfo(itemModel.currentItemInfo);
    }
}
