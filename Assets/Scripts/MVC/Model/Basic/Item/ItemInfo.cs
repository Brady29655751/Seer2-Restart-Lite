using System;
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

    public ItemInfo(string[] _data, int startIndex = 0) {
        string[] _slicedData = new string[DATA_COL];
        Array.Copy(_data, startIndex, _slicedData, 0, _slicedData.Length);
        id = int.Parse(_slicedData[0]);
        name = _slicedData[1];
        type = _slicedData[2].ToItemType();
        SetCurrencyTypeAndPrice(_slicedData[3]);
        options.ParseOptions(_slicedData[4]);
        itemDescription = (_slicedData[5] == "none") ? string.Empty : _slicedData[5].Replace("[ENDL]", "\n")
            .Replace("[-]", "</color>").Replace("[", "<color=#").Replace("]", ">");
        effectDescription = _slicedData[6].Trim().Replace("[ENDL]", "\n");

        resId = options.Get("resId", id.ToString());
        getId = int.Parse(options.Get("getId", id.ToString()));
        removable = bool.Parse(options.Get("removable", "true"));
    }

    public void SetEffects(List<Effect> _effects) {
        effects = _effects;
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

    public static Sprite GetIcon(string resId) {
        resId = resId.Trim();
        if (resId == "none")
            return null;

        if (int.TryParse(resId, out _))
            return ResourceManager.instance.GetLocalAddressables<Sprite>("Items/" + resId);

        var itemIcon = ResourceManager.instance.GetLocalAddressables<Sprite>(resId);
        if (itemIcon != null)
            return itemIcon;
        
        return ResourceManager.instance.GetLocalAddressables<Sprite>(resId, true);    
    }
}
