using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using UnityEngine;
using UnityEngine.UI;

public class PetItemBlockView : Module
{
    private Item item;
    private bool isNull => (item == null);

    [SerializeField] private IButton button;
    [SerializeField] private Image icon;
    [SerializeField] private Text numText;

    public void SetItem(Item item) {
        this.item = item;
        button?.SetInteractable(!isNull, false);
        icon?.gameObject.SetActive(!isNull);
        icon?.SetSprite(isNull ? null :   item.info.icon);
        numText?.SetText((isNull || (item.num < 0)) ? string.Empty : item.num.ToString());
    }

    public void SetRewardIcon(Sprite sprite) {
        button?.SetInteractable(sprite != null);
        icon?.gameObject.SetActive(sprite != null);
        icon?.SetSprite(sprite);
        numText?.SetText(string.Empty);
    }

    public void SetChosen(bool chosen) {
        
    }
}
