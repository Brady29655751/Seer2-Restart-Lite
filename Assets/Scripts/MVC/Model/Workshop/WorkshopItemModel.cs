using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SimpleFileBrowser;

public class WorkshopItemModel : Module
{
    [SerializeField] private IInputField idInputField, nameInputField;
    [SerializeField] private IDropdown typeDropdown;
    [SerializeField] private IInputField priceInputField, currencyTypeInputField;
    [SerializeField] private IInputField itemDescriptionInputField, effectDescriptionInputField, optionInputField;
    
    public ItemInfo itemInfo => GetItemInfo();
    public int id => int.Parse(idInputField.inputString);
    public string itemName => nameInputField.inputString;
    public ItemType type => (ItemType)(typeDropdown.value + 1);
    public int price => int.Parse(priceInputField.inputString);
    public int currencyType => int.Parse(currencyTypeInputField.inputString);
    
    private byte[] iconBytes = null;
    public Sprite iconSprite;
    public string itemDescription => itemDescriptionInputField.inputString?.Replace("\n", "[ENDL]") ?? string.Empty;
    public string effectDescription => effectDescriptionInputField.inputString?.Replace("\n", "[ENDL]") ?? string.Empty;
    public string options => string.IsNullOrEmpty(optionInputField.inputString) ? "none" : optionInputField.inputString;
    
    public List<Effect> effectList = new List<Effect>();
    public event Action<Sprite> onUploadIconEvent;

    public ItemInfo GetItemInfo() {
        var itemInfo = new ItemInfo(id, itemName, type, price, currencyType, options, itemDescription, effectDescription);
        itemInfo.SetEffects(effectList.Select(x => new Effect(x)).ToList());
        return itemInfo;
    }

    public void SetItemInfo(ItemInfo itemInfo) {
        idInputField.SetInputString(itemInfo.id.ToString());
        nameInputField.SetInputString(itemInfo.name);

        typeDropdown.value = ((int)itemInfo.type) - 1;

        priceInputField.SetInputString(itemInfo.price.ToString());
        currencyTypeInputField.SetInputString(itemInfo.currencyType.ToString());

        itemDescriptionInputField.SetInputString(itemInfo.itemDescription);
        effectDescriptionInputField.SetInputString(itemInfo.effectDescription);

        optionInputField.SetInputString(itemInfo.options.Select(entry => entry.Key + "=" + entry.Value).ConcatToString("&"));

        effectList = itemInfo.effects.Select(x => new Effect(x)).ToList();

        iconBytes = null;
        iconSprite = null;
    }

    public void OnClearIcon() {
        iconBytes = null;
    }

    public void OnUploadIcon() {
        var filter = new FileBrowser.Filter("图片", ".png");
        FileBrowser.SetFilters(false, filter);
        FileBrowser.ShowLoadDialog(OnUploadIconSuccess, OnUploadIconCancel, FileBrowser.PickMode.Files, title: "选择要上传的图片");
    }

    private void OnUploadIconSuccess(string[] paths) {
        if (!SaveSystem.TryLoadAllBytes(paths[0], out iconBytes)) {
            Hintbox.OpenHintboxWithContent("图片载入失败", 16);
            return;
        }

        if (!SpriteSet.TryCreateSpriteFromBytes(iconBytes, out iconSprite)) {
            Hintbox.OpenHintboxWithContent("创造道具图标失败", 16);
            return;
        }

        onUploadIconEvent?.Invoke(iconSprite);
    }

    private void OnUploadIconCancel() {
        // pass.
    }

    public void OnAddEffect(Effect effect) {
        effectList.Add(effect);
    }

    public void OnRemoveEffect() {
        if (effectList.Count == 0)
            return;

        effectList.RemoveAt(effectList.Count - 1);
    }

    public void OnEditEffect(int index, Effect effect) {
        if (!index.IsInRange(0, effectList.Count))
            return;

        effectList[index] = effect;
    }

    public bool CreateDIYItem(out string message) {
        var originalItemInfo = Item.GetItemInfo(itemInfo.id);
        Database.instance.itemInfoDict.Set(itemInfo.id, itemInfo);
        if (SaveSystem.TrySaveItemMod(itemInfo, iconBytes, iconSprite, out var error)) {
            message = "DIY写入成功";
            return true;
        }

        // rollback
        Database.instance.itemInfoDict.Set(itemInfo.id, originalItemInfo);
        message = "DIY写入失败\n" + error;
        return false;
    }

    public bool DeleteDIYItem(out string message) {
        if (!VerifyId(out message))
            return false;

        var originalItemInfo = Item.GetItemInfo(id);
        if ((originalItemInfo == null) || (!ItemInfo.IsMod(id))) {
            message = "未检测到此序号的Mod道具";
            return false;
        }

        Database.instance.itemInfoDict.Remove(id);
        if (SaveSystem.TrySaveItemMod(originalItemInfo, null, null, out var error, id)) {
            message = "道具删除成功";
            return true;
        }

        // rollback
        Database.instance.itemInfoDict.Set(id, originalItemInfo);
        message = "道具删除失败\n" + error;
        return false;
    }

    public bool VerifyDIYItem(out string error) {
        if (!VerifyId(out error))
            return false;

        if (!VerifyName(out error))
            return false;

        if (!VerifyPriceAndCurrencyType(out error))
            return false;

        if (!VerifyOptions(out error))
            return false;

        if (!VerifyIcon(out error))
            return false;

        if (!VerifyDescription(out error))
            return false;

        error = string.Empty;
        return true;
    }

    private bool VerifyId(out string error) {
        error = string.Empty;

        if (string.IsNullOrEmpty(idInputField.inputString)) {
            error = "序号不能为空！";
            return false;
        }

        if (!int.TryParse(idInputField.inputString, out _)) {
            error = "序号需为整数！";
            return false;
        }

        if (id >= 0) {
            error = "序号需为负数";
            return false;
        }

        return true;
    }

    private bool VerifyName(out string error) {
        error = string.Empty;

        if (string.IsNullOrEmpty(itemName)) {
            error = "名字不能为空！";
            return false;
        }

        if (itemName.Contains(',')) {
            error = "名字不能有半形逗号";
            return false;
        }

        return true;
    }

    private bool VerifyPriceAndCurrencyType(out string error) {
        error = string.Empty;

        if (string.IsNullOrEmpty(priceInputField.inputString)) {
            error = "价格不能为空！";
            return false;
        }

        if (!int.TryParse(priceInputField.inputString, out _)) {
            error = "价格必须为整数";
            return false;
        }

        if (string.IsNullOrEmpty(currencyTypeInputField.inputString)) {
            error = "交易货币种类不能为空！";
            return false;
        }

        if (!int.TryParse(currencyTypeInputField.inputString, out _)) {
            error = "交易货币种类必须为整数";
            return false;
        }

        if ((type == ItemType.Currency) && ((price != 1) || (currencyType != id))) {
            error = "货币类道具的价格必须为1，且交易货币种类必须为该道具自身的id";
            return false;
        }

        return true;
    }

    private bool VerifyOptions(out string error) {
        error = string.Empty;

        if (options.Contains(',')) {
            error = "【其他】部分不能有半形逗号";
            return false;
        }

        var dict = new Dictionary<string, string>();
        try {
            dict.ParseOptions(options);
        } catch (Exception) {
            error = "【其他】部分有重复或残缺的自定义选项";
            return false;
        }

        return true;
    }

    private bool VerifyIcon(out string error) {
        error = string.Empty;

        var dict = new Dictionary<string, string>();
        dict.ParseOptions(options);

        if (dict.ContainsKey("resId")) {
            var sprite = ItemInfo.GetIcon(dict.Get("resId"));
            if (sprite != null)
                return true;

            error = "【其他】的resId自定义选项引用的图案路径不存在";
            return false;
        }

        if ((Item.GetItemInfo(id) == null) && (iconBytes == null)) {
            error = "未指定道具图案或resId自定义选项";
            return false;
        }

        return true;
    }

    private bool VerifyDescription(out string error) {  
        error = string.Empty;

        /*
        if (string.IsNullOrEmpty(itemDescriptionInputField.inputString)) {
            error = "道具描述不能为空！";
            return false;
        }
        */

        if (itemDescriptionInputField.inputString.Contains(',')) {
            error = "道具描述不能有半形逗号";
            return false;
        }

        if (string.IsNullOrEmpty(effectDescriptionInputField.inputString)) {
            error = "效果描述不能为空！";
            return false;
        }

        if (effectDescriptionInputField.inputString.Contains(',')) {
            error = "效果描述不能有半形逗号";
            return false;
        }

        return true;
    }


}
