using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CookPanel : Panel
{
    [SerializeField] private CookController cookController;
    [SerializeField] private IText titleText;
    [SerializeField] private Text cookText;
    private bool isSpecialRecipe = false;
    

    public override void Init() {
        if (isSpecialRecipe)
            return;

        cookController.SetCookItemStorage(Database.instance.itemInfoDict.Keys.Where(x => x.IsInRange(200000, 210000)).Select(x => new Item(x, -1)).ToList());
    }

    public override void SetPanelIdentifier(string id, string param) {
        switch (id) {
            default:
                base.SetPanelIdentifier(id, param);
                return;
            case "recipe":
                cookController.SetCookItemStorage(param.ToIntList('/').Select(x => new Item(x, -1)).ToList());
                isSpecialRecipe = true;
                return;
            case "title":
                titleText?.SetText(param);
                return;
            case "cookText":
                cookText?.SetText(param);
                return;
        }
    }
    
}
