using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PetBagModel : Module
{
    private PetBagMode mode = PetBagMode.Normal;
    private Pet[] petBag => GetPetBag();
    private List<Pet> petStorage => Player.instance.gameData.petStorage;
    
    [SerializeField] private PetSelectModel selectModel;

    private Pet[] GetPetBag() {
        return mode switch {
            PetBagMode.Normal => Player.instance.gameData.petBag,
            PetBagMode.PVP => selectModel.selections,
            _ => Player.instance.gameData.petBag,
        };
    }

    public void SetMode(PetBagMode newMode) {
        mode = newMode;
    }

    public void SetPetSwap(int indexA, int indexB) {
        petBag.Swap(indexA, indexB);
        if (mode == PetBagMode.Normal)
            SaveSystem.SaveData();
    }

    public void SetPetDrop(int dropIndex) {
        SetPetSwap(selectModel.startDragIndex, dropIndex);
    }

    public void SetPetFirst() {
        if (selectModel.cursor.Length <= 0)
            return;

        SetPetSwap(0, selectModel.cursor[0]);
    }

    public void SetPetHeal() {
        foreach (var p in selectModel.selections) {
            if (p != null)
                p.currentStatus.hp = p.normalStatus.hp;
        }
        
        if (mode == PetBagMode.Normal)
            SaveSystem.SaveData();
    }

    public Pet SetPetTrain() {
        if (selectModel.cursor.Length <= 0)
            return null;

        Pet pet = Pet.ToBestPet(selectModel.currentSelectedItems[0]);        

        if (mode == PetBagMode.Normal)
            SaveSystem.SaveData();

        return pet;
    }

    public bool SetPetHome() {
        int petBagCount = selectModel.selections.Count(x => x != null);
        if (petBagCount <= 1) {
            return false;
        }
        if (selectModel.cursor.Length <= 0)
            return false;

        int index = selectModel.cursor[0];
        Pet pet = selectModel.currentSelectedItems[0];
        petStorage.Add(pet);
        petBag.MoveRangeTo(index + 1, petBag.Length, index);
        petBag[petBag.Length - 1] = null;

        if (mode == PetBagMode.Normal)
            SaveSystem.SaveData();

        return true;
    }
}
