using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PetStorageModel : Module
{
    private PetBagMode mode;
    public Pet[] petBag => PetStoragePanel.IsNormalStorageMode(mode) ? Player.instance.gameData.petBag : petBagPanel.petBag;
    public List<Pet> petStorage => PetStoragePanel.GetDefaultPetStorage(mode);

    [SerializeField] private PetSelectModel storageSelectModel;
    [SerializeField] private PetBagPanel petBagPanel = null;

    public int storageSelectRefreshPage => storageSelectModel.GetRefreshPageAfterRemoved();
    public int storageSelectPage => storageSelectModel.page;
    public int storageSelectCursor => ((storageSelectModel.cursor.Length > 0) ?
        storageSelectModel.cursor[0] : 0);

    public void SetMode(PetBagMode mode)
    {
        this.mode = mode;
    }

    public bool OnPetTake()
    {
        if (storageSelectModel.cursor.Length <= 0)
            return false;

        if (!petBag.Contains(null))
            return false;

        if (mode == PetBagMode.PVP)
        {
            var petCount = (int)PhotonNetwork.CurrentRoom.CustomProperties["count"];
            if (petBag.Count(x => x != null) == petCount)
                return false;
        }
        SetPetTake(null);
        return true;
    }

    public void SetPetTake(Pet oldPet)
    {
        Pet newPet = storageSelectModel.currentSelectedItems[0];

        if (!PetStoragePanel.IsNormalStorageMode(mode))
        {
            petBag.Update(oldPet, newPet);
            petBagPanel?.RefreshPetBag();
            return;
        }

        Player.instance.gameData.petStorage.Remove(newPet);
        if (oldPet != null)
            Player.instance.gameData.petStorage.Add(oldPet);

        petBag.Update(oldPet, newPet);
        SaveSystem.SaveData();
    }

    public void SetPetRelease()
    {
        if ((storageSelectModel.cursor.Length <= 0) || !PetStoragePanel.IsNormalStorageMode(mode))
            return;

        Pet pet = storageSelectModel.currentSelectedItems[0];
        Player.instance.gameData.petStorage.Remove(pet);
        SaveSystem.SaveData();

    }

    public void SetPetElite()
    {
        if (storageSelectModel.cursor.Length <= 0)
            return;

        Pet pet = storageSelectModel.currentSelectedItems[0];
        if (!pet.record.GetRecord("elite", false))
        {
            pet.record.SetRecord("elite", true);
            SaveSystem.SaveData();
        }
        else
        {
            Hintbox.OpenHintboxWithContent("该精灵已经在精英仓库了哦！", 16);
        }
    }

    public void SetPetRemoveElite()
    {
        if (storageSelectModel.cursor.Length <= 0)
            return;

        Pet pet = storageSelectModel.currentSelectedItems[0];
        if (!pet.record.GetRecord("elite", false))
        {
            Hintbox.OpenHintboxWithContent("该精灵本来就不在精英仓库哦！", 16);
        }
        else
        {
            pet.record.SetRecord("elite", false);
            SaveSystem.SaveData();
        }
    }
}
