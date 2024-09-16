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
                SetIcon(SpriteSet.GetIconSprite(param));
                break;
        }
    }

    public void SetIcon(Sprite sprite) {
        icon?.SetSprite(sprite);
    }
}
