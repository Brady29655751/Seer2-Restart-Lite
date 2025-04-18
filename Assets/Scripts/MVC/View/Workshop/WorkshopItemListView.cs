using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class WorkshopItemListView : Module
{
    [SerializeField] private InfoPrompt infoPrompt;
    [SerializeField] private GameObject itemInfoCellPrefab;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private RectTransform contentRect;

    private List<ItemCell> itemInfoCellList = new List<ItemCell>();

    public void CreateItemInfoList(List<ItemInfo> itemInfos, Action<ItemInfo> callback = null) {
        StartCoroutine(CreateItemInfoListCoroutine(itemInfos, callback));
    }

    private IEnumerator CreateItemInfoListCoroutine(List<ItemInfo> itemInfos, Action<ItemInfo> callback = null) {
        var loadingScreen = SceneLoader.instance.ShowLoadingScreen(-1, "正在加载道具");
        var itemInfosCreatedEachTime = Mathf.Max(400, Mathf.FloorToInt(itemInfos.Count * 0.05f));
        for (int i = 0; i < itemInfos.Count; i += itemInfosCreatedEachTime) {
            float progress = i * 1f / itemInfos.Count;
            itemInfoCellList?.AddRange(itemInfos.GetRange(i, Mathf.Min(itemInfosCreatedEachTime, itemInfos.Count - i)).Select(x => CreateItemInfoCell(x, callback)));
            loadingScreen.SetText("正在加载道具 " + i + " / " + itemInfos.Count);
            loadingScreen.ShowLoadingProgress(progress);
            yield return null;
        }
        SceneLoader.instance.HideLoadingScreen();
    }

    private ItemCell CreateItemInfoCell(ItemInfo itemInfo, Action<ItemInfo> callback) {
        var obj = GameObject.Instantiate(itemInfoCellPrefab, contentRect);
        var cell = obj?.GetComponent<ItemCell>();

        cell?.SetInfoPrompt(infoPrompt);
        cell?.SetItemInfo(itemInfo);
        cell?.SetCallback(callback);
        return cell;
    }

    public void ShowResult(List<ItemInfo> itemInfos, bool isIdFilter) {
        if (isIdFilter) {
            ScrollToItemInfo(itemInfos?.FirstOrDefault());
            return;
        }
            
        SetItemInfoList(itemInfos);
    }

    public void SetItemInfoList(List<ItemInfo> itemInfos) {
        for (int i = 0; i < itemInfoCellList.Count; i++)
            itemInfoCellList[i].SetItemInfo((i < itemInfos.Count) ? itemInfos[i] : null);
    }

    public void ScrollToItemInfo(ItemInfo itemInfo) {
        if (itemInfo == null) {
            scrollRect.verticalNormalizedPosition = 1;
            return;
        }

        var cell = itemInfoCellList?.Find(x => (x != null) && (itemInfo.id == x.currentItemInfo.id));
        if (cell == null) {
            scrollRect.verticalNormalizedPosition = 1;
            return;
        }

        var pos = cell.rectTransform.anchoredPosition;
        scrollRect.verticalNormalizedPosition = 1 + pos.y / contentRect.rect.size.y;
    }
}
