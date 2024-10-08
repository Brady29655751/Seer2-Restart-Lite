using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CookPanel : Panel
{
    [SerializeField] private CookController cookController;
    
    public override void Init() {
        cookController.SetCookItemStorage(Database.instance.itemInfoDict.Keys.Where(x => x.IsInRange(200000, 300000)).Select(x => new Item(x, -1)).ToList());
    }

    public override void SetPanelIdentifier(string id, string param) {
        switch (id) {
            default:
                base.SetPanelIdentifier(id, param);
                return;
            case "recipe":
                cookController.SetCookItemStorage(param.ToIntList('/').Select(x => new Item(x, -1)).ToList());
                return;
        }
    }
    
}
