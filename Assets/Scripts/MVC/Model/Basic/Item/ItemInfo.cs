using System;
using System.Linq;
using System.Xml.Serialization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RM = ResourceManager;
using System.Threading.Tasks;
using UnityEngine.AddressableAssets;

public class ItemInfo
{
    public const int DATA_COL = 7;

    public int id { get; private set; }
    public string resId { get; private set; }
    public int getId { get; private set; }
    public string name { get; private set; }
    public ItemType type { get; private set; }
    public int currencyType { get; private set; }
    public int price { get; private set; }
    public string itemDescription { get; private set; }
    public string effectDescription { get; private set; }
    public Dictionary<string, string> options { get; private set; } = new Dictionary<string, string>();
    public List<Effect> effects { get; private set; } = new List<Effect>();

    public Sprite icon => GetIcon(resId);
    public ItemInfo currencyInfo => Item.GetItemInfo(currencyType);
    public bool removable = true;

    public static bool IsMod(int id) => id < 0;

    public ItemInfo(string[] _data, int startIndex = 0) {
        string[] _slicedData = new string[DATA_COL];
        Array.Copy(_data, startIndex, _slicedData, 0, _slicedData.Length);
        id = int.Parse(_slicedData[0]);
        name = _slicedData[1];
        type = _slicedData[2].ToItemType();
        SetCurrencyTypeAndPrice(_slicedData[3]);
        options.ParseOptions(_slicedData[4]);
        InitOptionProperty();
        itemDescription = (_slicedData[5] == "none") ? string.Empty : _slicedData[5];
        effectDescription = _slicedData[6].Trim();
    }

    public ItemInfo(int id, string itemName, ItemType type, int price, int currencyType, 
        string options, string itemDescription, string effectDescription) {
        this.id = id;
        this.name = itemName.ReplaceSpecialWhiteSpaceCharacters(string.Empty);
        this.type = type;
        this.currencyType = currencyType;
        this.price = price;
        this.options.ParseOptions(options.ReplaceSpecialWhiteSpaceCharacters(string.Empty));
        InitOptionProperty();
        this.itemDescription = (itemDescription == "none") ? string.Empty : itemDescription.ReplaceSpecialWhiteSpaceCharacters(string.Empty);
        this.effectDescription = effectDescription.ReplaceSpecialWhiteSpaceCharacters(string.Empty);
    }

    public void SetEffects(List<Effect> _effects) {
        effects = _effects;
    }

    public void InitOptionProperty() {
        resId = options.Get("resId", id.ToString());
        getId = int.Parse(options.Get("getId", id.ToString()));
        removable = bool.Parse(options.Get("removable", "true"));
    }

    public void SetCurrencyTypeAndPrice(string priceTag) {
        int startIndex = priceTag.IndexOf('[');
        int endIndex = priceTag.IndexOf(']');
        if ((startIndex == -1) || (endIndex == -1)) {
            currencyType = Item.COIN_ID;
            price = int.Parse(priceTag);
            return;
        }

        string currency = priceTag.Substring(startIndex + 1, endIndex - startIndex - 1);
        string value = priceTag.Substring(0, startIndex);

        currencyType = (currency == ("D")) ? Item.DIAMOND_ID : int.Parse(currency);
        price = int.Parse(value);
    }

    public string GetItemDescription() {
        var itemDesc = itemDescription;
        if (!removable)
            itemDesc = "[ffbb33]【无限再生】[-][ENDL]" + itemDesc;

        return itemDesc.ReplaceColorAndNewline();
    }

    public string GetEffectDescription() => effectDescription.ReplaceColorAndNewline();

    public string[] GetRawInfoStringArray() {
        var itemDesc = string.IsNullOrEmpty(itemDescription) ? "none" : itemDescription;
        var optionsAll = options.Select(entry => entry.Key + "=" + entry.Value).ConcatToString("&");
        optionsAll = string.IsNullOrEmpty(optionsAll) ? "none" : optionsAll;

        return new string[] { id.ToString(), name, type.ToRawString(), price + "[" + currencyType + "]",
           optionsAll, itemDesc, effectDescription };
    }

    public static Sprite GetIcon(string resId) {
        resId = resId.Trim();
        if (resId == "none")
            return null;

        if (int.TryParse(resId, out var iconId))
            return ResourceManager.instance.GetLocalAddressables<Sprite>("Items/" + resId, IsMod(iconId));

        var itemIcon = ResourceManager.instance.GetLocalAddressables<Sprite>(resId);
        if (itemIcon != null)
            return itemIcon;
        
        return ResourceManager.instance.GetLocalAddressables<Sprite>(resId, true);    
    }
}
