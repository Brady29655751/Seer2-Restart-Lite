using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WorkshopLearnItemView : UIModule
{
    [SerializeField] private PetItemBlockView itemBlockView;
    [SerializeField] private RectTransform selectItemContentRect;
    [SerializeField] private GameObject selectItemButtonPrefab;

    private List<GameObject> selectItemButtonPrefabList = new List<GameObject>();

    public void SetSelections(List<ItemInfo> selections, Action<int> callback) {
        selectItemButtonPrefabList.ForEach(Destroy);
        selectItemButtonPrefabList = selections.Select((x, i) => { 
            int copy = i;
            var obj = Instantiate(selectItemButtonPrefab, selectItemContentRect);
            obj.GetComponent<IButton>()?.onPointerClickEvent.SetListener(() => callback?.Invoke(copy));
            obj.GetComponentInChildren<Text>()?.SetText(selections[copy].id + "/" + selections[copy].name);
            return obj;
        }).ToList();

        // Clear current preview
        OnPreviewItem(null);
    }

    public void OnPreviewItem(ItemInfo itemInfo) {
        var item = (itemInfo == null) ? null : new Item(itemInfo.id, -1);
        itemBlockView.SetItem(item);
    }

    public void ShowItemInfo(ItemInfo itemInfo) {
        var item = (itemInfo == null) ? null : new Item(itemInfo.id, -1);
        if (item == null) {
            SetInfoPromptActive(false);
            return;
        }
        infoPrompt.SetItem(item);
    }

}
