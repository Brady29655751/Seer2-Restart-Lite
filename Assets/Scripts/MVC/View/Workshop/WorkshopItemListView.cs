using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class WorkshopItemListView : UIModule
{
    [SerializeField] private List<ItemCell> itemInfoCellList = new List<ItemCell>();

    public void CreateItemInfoList(List<ItemInfo> itemInfos, Action<ItemInfo> callback = null) {
        for (int i = 0; i < itemInfoCellList.Count; i++)
        {
            var cell = itemInfoCellList[i];
            cell.SetInfoPrompt(infoPrompt);   
            cell.SetItemInfo(itemInfos.Get(i));
            cell.SetCallback(callback, "id");
        }
    }


    public void SetItemInfoList(List<ItemInfo> itemInfos) {
        for (int i = 0; i < itemInfoCellList.Count; i++)
            itemInfoCellList[i].SetItemInfo(itemInfos.Get(i));
    }

}
