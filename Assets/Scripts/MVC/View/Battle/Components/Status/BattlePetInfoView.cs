using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattlePetInfoView : BattleBaseView
{
    [SerializeField] private bool nameFontSizeFit = false;
    [SerializeField] private bool levelTextWithLV = false;
    [SerializeField] private Image icon;
    [SerializeField] private Text nameText;
    [SerializeField] private Text levelText;
    [SerializeField] private Image elementImage, subElementImage;
    [SerializeField] private GameObject subElementObject;

    public void SetPet(BattlePet pet) {
        var isSubElementActive = pet.subBattleElement != Element.普通;
        icon.sprite =   pet.ui.icon;
        nameText.text = pet.name;
        
        if (nameFontSizeFit)
            nameText.fontSize = (pet.name.Length < 6) ? 12 : 10;

        levelText.text = (levelTextWithLV ? "LV " : string.Empty)  + pet.level.ToString();
        elementImage.SetElementSprite(pet.battleElement);
        
        //! Do not use ?. here beacuse null check of Gameobject is special
        if (subElementObject != null)
            subElementObject.SetActive(isSubElementActive);
        
        if (subElementImage != null)
            subElementImage.SetElementSprite(pet.subBattleElement);
    }

}
