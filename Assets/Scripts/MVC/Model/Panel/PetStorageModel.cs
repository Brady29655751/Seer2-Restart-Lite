using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PetStorageModel : Module
{
    private Pet[] petBag => Player.instance.gameData.petBag;
    private List<Pet> petStorage => Player.instance.gameData.petStorage;
    [SerializeField] private PetSelectModel storageSelectModel;

    public int storageSelectRefreshPage => storageSelectModel.GetRefreshPageAfterRemoved();
    public int storageSelectPage => storageSelectModel.page;
    public int storageSelectCursor => ((storageSelectModel.cursor.Length > 0) ? 
        storageSelectModel.cursor[0] : 0);

    public bool OnPetTake() {
        if (storageSelectModel.cursor.Length <= 0)
            return false;

        if (!petBag.Contains(null))
            return false;

        SetPetTake(null);
        return true;
    }

    public void SetPetTake(Pet oldPet) {
        Pet newPet = storageSelectModel.currentSelectedItems[0];
        
        petStorage.Remove(newPet);
        if (oldPet != null) {
            petStorage.Add(oldPet);
        }
        petBag.Update(oldPet, newPet);
        SaveSystem.SaveData();
    }

    public void SetPetRelease() {
        if (storageSelectModel.cursor.Length <= 0)
            return;

        Pet pet = storageSelectModel.currentSelectedItems[0];
        petStorage.Remove(pet);
        SaveSystem.SaveData();

    }
}
