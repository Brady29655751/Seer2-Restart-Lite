using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PetInfoModel : Module
{
    private Pet currentPet;

    public int id => currentPet.id;
    public int level => currentPet.level;
    public int evolveLevel => currentPet.info.exp.evolveLevel;
    public uint levelUpExp => currentPet.levelUpExp;
    public Personality personality => currentPet.basic.personality;
    public int height => currentPet.basic.height;
    public int weight => currentPet.basic.weight;
    public DateTime getPetDate => currentPet.basic.getPetDate;

    public void SetPet(Pet pet) {
        currentPet = pet;
    }

}
