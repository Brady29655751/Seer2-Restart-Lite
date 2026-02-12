using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorkshopItemListController : Module
{
    [SerializeField] private WorkshopItemListModel itemListModel;
    [SerializeField] private WorkshopItemListView itemListView;
    [SerializeField] private PageView pageView;
    [SerializeField] private WorkshopAllController allController;

    public override void Init()
    {
        base.Init();
        itemListModel.SetStorage(ItemInfo.database);
        itemListView.CreateItemInfoList(ItemInfo.database, OnEditItem);
        OnSetPage();
    }

    private void OnEditItem(ItemInfo itemInfo) {
        allController?.OnEditItem(itemInfo);
        SetActive(false);
    }

    public void SetActive(bool active) {
        itemListModel.SetActive(active);
        if (!active) 
            OnSetPage();
    }

    public void Search() 
    {
        itemListModel.Search();
        OnSetPage();
    }

    public void Sort(string key)
    {
        itemListModel.Sort(key);
        OnSetPage();
    }

    private void OnSetPage()
    {
        itemListView.SetItemInfoList(itemListModel.selections.ToList());
        pageView.SetPage(itemListModel.page, itemListModel.lastPage);
    }

    public void PrevPage()
    {
        itemListModel.PrevPage();
        OnSetPage();
    }

    public void NextPage()
    {
        itemListModel.NextPage();
        OnSetPage();
    }

}
