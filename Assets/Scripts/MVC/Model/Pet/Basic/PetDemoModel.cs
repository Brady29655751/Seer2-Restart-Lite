using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PetDemoModel : Module
{
    private Pet currentPet;

    public string petName => currentPet.name;

    public Element element => currentPet.element;
    public Element subElement => currentPet.subElement;
    public Feature feature => currentPet.feature.feature;
    public Emblem emblem => currentPet.feature.emblem;
    public int iv => currentPet.talent.iv;
    public IVRanking ivRank => currentPet.talent.IVRank;

    public void SetPet(Pet pet) {
        currentPet = pet;
    }


    
}
