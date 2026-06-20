using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemBagPanel : Panel
{
    [SerializeField] private PetItemController itemController;
    [SerializeField] private Text titleText;

    public override void SetPanelIdentifier(string id, string param)
    {
        switch (id)
        {
            default:
                base.SetPanelIdentifier(id, param);
                return;

            case "title":
                titleText?.SetText(param);
                return;

            case "item":
                var itemList = new List<int>();
                if (param.TryTrimParentheses(out _, '(', ')'))
                {
                    var itemFilter = Parser.ParseConditionFilter<ItemInfo>(param, (type, info) =>
                        info.TryGetItemInfoIdentifier(type, out var num) ? num : Identifier.GetNumIdentifier(type)
                    );
                    itemList = ItemInfo.database.Where(itemFilter).Select(x => x.id).ToList();
                }
                else
                {
                    itemList = param.Split('/').Select(x => (int)Parser.ParseOperation(x)).ToList();
                }
                SetItemBag(itemList.Select(id => Item.Find(id) ?? new Item(id, 0)).ToList());
                break;
                
            case "mode":
                switch (param)
                {
                    default:
                        return;

                    case "animal":
                        var animalItems = new List<Item>();
                        var animalList = Item.animalItemDatabase.Concat(Item.animalProductItemDatabase);
                        foreach (var animal in animalList)
                        {
                            animalItems.Add(Item.Find(animal.id) ?? new Item(animal.id, 0));
                        }
                        SetItemBag(animalItems);
                        return;
                }
        }

    }

    public void SetItemBag(List<Item> items)
    {
        itemController.SetItemBag(items);
    }
}
