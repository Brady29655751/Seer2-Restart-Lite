using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemHintbox : Hintbox
{
    [SerializeField] private Image icon;

    public override void SetPanelIdentifier(string id, string param) {
        switch (id) {
            default:
                base.SetPanelIdentifier(id, param);
                break;
            case "item_icon":
                Sprite icon = null;
                var splitIndex = param.IndexOf('[');
                if (splitIndex != -1) {
                    var category = param.Substring(0, splitIndex).ToLower();
                    if (int.TryParse(param.TrimParentheses(), out int iconId)) {
                        icon = category switch {
                            "pet" =>   Pet.GetPetInfo(iconId)?.ui.icon,
                            "item" =>   Item.GetItemInfo(iconId)?.icon,
                            "emblem" =>   Pet.GetPetInfo(iconId)?.ui.emblemIcon,
                            _ =>   NpcInfo.GetIcon(param),
                        };
                    }
                }
                icon ??= NpcInfo.GetIcon(param);
                SetIcon(icon);
                break;
        }
    }

    public void SetIcon(Sprite sprite) {
        icon?.SetSprite(sprite);
    }
}
