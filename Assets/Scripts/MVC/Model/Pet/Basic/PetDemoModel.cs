using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PetDemoModel : Module
{
    public Pet currentPet { get; private set; }
    public bool animMode { get; private set; } = true;

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

    public void SetAnimMode(bool newMode) {
        animMode = newMode;
    }

    
}
