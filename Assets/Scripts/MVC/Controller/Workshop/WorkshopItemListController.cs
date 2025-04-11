using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorkshopItemListController : Module
{
    [SerializeField] private WorkshopItemListModel itemListModel;
    [SerializeField] private WorkshopItemListView itemListView;
    [SerializeField] private WorkshopAllController allController;

    public override void Init()
    {
        base.Init();
        itemListModel.SetItemInfoList(ItemInfo.database);
        itemListView.CreateItemInfoList(ItemInfo.database, OnEditItem);
    }

    private void OnEditItem(ItemInfo itemInfo) {
        allController?.OnEditItem(itemInfo);
        SetActive(false);
    }

    public void SetActive(bool active) {
        itemListModel.SetActive(active);
        if (!active) 
            itemListView.SetItemInfoList(ItemInfo.database);
    }

    public void Search() {
        itemListView.ShowResult(itemListModel.resultItemInfoList, itemListModel.isIdFilter);
    }

    public void Sort(string type) {
        itemListModel.Sort(type);
        itemListView.ShowResult(itemListModel.resultItemInfoList, false);
    }
}
