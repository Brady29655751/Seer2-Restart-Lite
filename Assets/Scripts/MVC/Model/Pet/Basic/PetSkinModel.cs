using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PetSkinModel : SelectModel<int>
{
    public Pet currentPet;
    public PetUIInfo uiInfo => currentPet.info.ui;
    public int[] skinIds => selections;
    public int currentSkinId => currentSelectedItems[0];

    public void SetPet(Pet pet) {
        currentPet = pet;
        SetStorage(uiInfo.GetAllSkinList(pet.ui));
        SetPage(0);
    }

    public void SetSkin() {
        if (cursor.Length == 0)
            return;

        currentPet.ui.skinId = currentSkinId;
        SaveSystem.SaveData();
    }
}
