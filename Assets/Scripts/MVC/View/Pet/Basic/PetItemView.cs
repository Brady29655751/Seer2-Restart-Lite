using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PetItemView : Module
{
    [SerializeField] private InfoPrompt infoPrompt;
    [SerializeField] private List<PetItemBlockView> itemBlockViews;

    public void SetItems(List<Item> items) {
        for (int i = 0; i < itemBlockViews.Count; i++) {
            itemBlockViews[i].SetItem((i < items.Count) ? items[i] : null);
        }
    }

    public void Select(int index) {
        for (int i = 0; i < itemBlockViews.Count; i++) {
            itemBlockViews[i].SetChosen(i == index);
        }
    }

    public void SetInfoPromptActive(bool active) {
        infoPrompt.SetActive(active);
    }

    public void ShowItemInfo(Item item) {
        if (item == null)
            return;

        infoPrompt.SetItem(item);
    }
}
