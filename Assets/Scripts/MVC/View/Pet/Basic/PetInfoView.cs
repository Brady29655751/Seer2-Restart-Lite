using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PetInfoView : Module
{
    [SerializeField] private Text idText;
    [SerializeField] private Text levelText;
    [SerializeField] private Text evolveLevelText;
    [SerializeField] private Text expLevelUpText;
    [SerializeField] private Text personalityText;
    [SerializeField] private Text heightText;
    [SerializeField] private Text weightText;
    [SerializeField] private Text getPetDateText;
    
    public void SetPet(Pet pet) {
        gameObject.SetActive(pet != null);
        if (pet == null)
            return;
            
        SetID(pet.id);
        SetLevel(pet.level);
        SetEvolveLevel(pet.info.exp.evolveLevel);
        SetLevelUpExp((pet.level >= 100) ? 0 : pet.levelUpExp);
        SetPersonality(pet.basic.personality);
        SetHeightAndWeight(pet.basic.height, pet.basic.weight);
        SetGetPetDate(pet.basic.getPetDate);
    }

    public void SetID(int id) {
        idText?.SetText(id.ToString());
    }

    public void SetLevel(int level) {
        levelText?.SetText(level.ToString());
    }

    public void SetEvolveLevel(int evolveLevel) {
        evolveLevelText?.SetText((evolveLevel <= 0) ? "--" : evolveLevel.ToString());
    }

    public void SetLevelUpExp(uint levelUpExp) {
        expLevelUpText?.SetText((levelUpExp <= 0) ? "--" : levelUpExp.ToString());
    }

    public void SetPersonality(Personality personality) {
        personalityText?.SetText(personality.ToString());
    }

    public void SetHeightAndWeight(int height, int weight) {
        heightText?.SetText(height + " cm");
        weightText?.SetText(weight + " kg");
    }

    public void SetGetPetDate(DateTime getPetDate) {
        getPetDateText?.SetText(getPetDate.ToString("yyyy年MM月dd日"));
    }

}
