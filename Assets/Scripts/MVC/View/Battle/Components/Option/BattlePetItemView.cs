using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattlePetItemView : BattleBaseView
{
    [SerializeField] private List<PetItemBlockView> itemBlockViews;

    public void SetItems(List<Item> items) {
        for (int i = 0; i < itemBlockViews.Count; i++) {
            itemBlockViews[i].SetItem((i < items.Count) ? items[i] : null);
        }
    }

    public void ShowItemInfo(int index, Item item) {
        if (item == null) 
            return;
        
        string header = "<size=4>\n</size><size=16><color=#52e5f9>" + item.name + "</color></size><size=6>\n\n</size>";
        string content = item.info.effectDescription;
        descriptionBox.SetBoxPosition(new Vector2(100 + index * 80, 109));
        descriptionBox.SetText(header + content);
        descriptionBox.SetBoxSize(Vector2.one * 150, true, true);
    }
}
