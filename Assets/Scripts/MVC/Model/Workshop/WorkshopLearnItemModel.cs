using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorkshopLearnItemModel : Module
{
    public const int MAX_SEARCH_COUNT = 50;
    [SerializeField] private IInputField searchInputField, idInputField;

    public int id => (int.TryParse(idInputField.inputString, out var itemId)) ? itemId : 0;

    public List<ItemInfo> itemInfoList = new List<ItemInfo>();
    public ItemInfo currentItemInfo => Item.GetItemInfo(id);

    public void Search() {
        if (string.IsNullOrEmpty(searchInputField.inputString)) {
            Hintbox.OpenHintboxWithContent("搜索名称不能为空！", 16);
            return;
        }

        idInputField.SetInputString(string.Empty);

        // Try search items with same name.
        var itemInfoDict = Database.instance.itemInfoDict;
        Func<KeyValuePair<int, ItemInfo>, bool> predicate = int.TryParse(searchInputField.inputString, out var itemId) ?
            new Func<KeyValuePair<int, ItemInfo>, bool>(x => x.Key == itemId) : 
            new Func<KeyValuePair<int, ItemInfo>, bool>(x => x.Value.name == searchInputField.inputString);

        itemInfoList = itemInfoDict.Where(predicate)
            .OrderBy(x => x.Key).Select(x => x.Value).Take(MAX_SEARCH_COUNT).ToList();

        if (itemInfoList.Count > 0)
            return;

        // No items with same name. Search similar result.
        itemInfoList = itemInfoDict.Where(x => x.Value.name.Contains(searchInputField.inputString))
            .OrderBy(x => x.Key).Select(x => x.Value).Take(MAX_SEARCH_COUNT).ToList();
    }

    public void Select(int index) {
        if (!index.IsInRange(0, itemInfoList.Count)) {
            idInputField.SetInputString(string.Empty);
            return;
        }

        idInputField.SetInputString(itemInfoList[index]?.id.ToString() ?? string.Empty);
    }

    public bool VerifyDIYLearnItem(out string error) {
        error = string.Empty;

        if (string.IsNullOrEmpty(idInputField.inputString)) {
            error = "道具序号不能为空！";
            return false;
        }

        if (Item.GetItemInfo(id) == null) {
            error = "该道具序号不存在！";
            return false;
        }

        return true;
    }
}
