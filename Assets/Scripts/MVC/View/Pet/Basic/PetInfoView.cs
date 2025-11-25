using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PetInfoView : Module
{
    [SerializeField] private bool showPersonalityDetail = false;
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
            
        SetID(pet.info.ui.defaultId, pet.info.ui.subId, pet.info.star, pet.backupPet != null);
        SetLevel(pet.level);
        SetEvolveLevel(pet.info.exp.evolveLevel);
        SetLevelUpExp((pet.level >= pet.maxLevel) ? 0 : pet.levelUpExp);
        SetPersonality(pet.basic.personality);
        SetHeightAndWeight(pet.basic.height, pet.basic.weight);
        SetGetPetDate(pet.basic.getPetDate);
    }

    public void SetID(int id, int subId, int star, bool isDevolved) {
        var subIdText = (subId == 0) ? string.Empty : ("-" + subId);
        var starColor = isDevolved ? "ff0080" : "ffbb33";
        var starText = $"<color=#{starColor}>(★<size=4> </size>" + star + "<size=4> </size>)</color>";
        idText?.SetText($"{id}{subIdText} {starText}");
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
        var desc = Status.GetPersonalityBuffDescription(personality, " ", string.Empty);
        var detail = showPersonalityDetail ? (" <color=#ffbb33>(" + desc + ")</color>") : string.Empty;
        personalityText?.SetText(personality.ToString());
    }

    public void SetHeightAndWeight(int height, int weight) {
        heightText?.SetText(height + " cm");
        weightText?.SetText(weight + " kg");
    }

    public void SetGetPetDate(DateTime getPetDate) {
        getPetDateText?.SetText(getPetDate.ToString("yyyy/MM/dd"));
    }

}
