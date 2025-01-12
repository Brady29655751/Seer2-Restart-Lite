using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PetStorageModel : Module
{
    private PetBagMode mode;
    public Pet[] petBag => (mode == PetBagMode.Normal) ? Player.instance.gameData.petBag : petBagPanel.petBag;
    public List<Pet> petStorage => PetStoragePanel.GetDefaultPetStorage(mode);

    [SerializeField] private PetSelectModel storageSelectModel;
    [SerializeField] private PetBagPanel petBagPanel = null;

    public int storageSelectRefreshPage => storageSelectModel.GetRefreshPageAfterRemoved();
    public int storageSelectPage => storageSelectModel.page;
    public int storageSelectCursor => ((storageSelectModel.cursor.Length > 0) ? 
        storageSelectModel.cursor[0] : 0);

    public void SetMode(PetBagMode mode) {
        this.mode = mode;
    }

    public bool OnPetTake() {
        if (storageSelectModel.cursor.Length <= 0)
            return false;

        if (!petBag.Contains(null))
            return false;
        
        if (mode == PetBagMode.PVP) {
            var petCount = (int)PhotonNetwork.CurrentRoom.CustomProperties["count"];
            if (petBag.Count(x => x != null) == petCount)
                return false;
        }
        SetPetTake(null);
        return true;
    }

    public void SetPetTake(Pet oldPet) {
        Pet newPet = storageSelectModel.currentSelectedItems[0];
        
        if (mode != PetBagMode.Normal) {
            petBag.Update(oldPet, newPet);
            petBagPanel?.RefreshPetBag();
            return;
        }
        petStorage.Remove(newPet);
        if (oldPet != null)
            petStorage.Add(oldPet);
        
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
