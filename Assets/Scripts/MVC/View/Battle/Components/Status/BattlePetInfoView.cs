using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattlePetInfoView : BattleBaseView
{
    [SerializeField] private bool levelTextWithLV = false;
    [SerializeField] private Image icon;
    [SerializeField] private Text nameText;
    [SerializeField] private Text levelText;
    [SerializeField] private Image elementImage;

    public void SetPet(BattlePet pet) {
        icon.sprite =   pet.ui.icon;
        nameText.text = pet.name;
        levelText.text = (levelTextWithLV ? "LV " : string.Empty)  + pet.level.ToString();
        elementImage.SetElementSprite(pet.element);
    }

}
