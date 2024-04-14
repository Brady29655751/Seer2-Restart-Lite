using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PetPersonalityModel : Module
{
    private Pet currentPet = null;
    public Personality personality { get; private set; }
    public Status personalityBuff => Status.GetPersonalityBuff(personality);

    public void SetPet(Pet pet) {
        currentPet = pet;
        this.personality = pet.basic.personality;
    }

    public void SetPersonality(Personality personality) {
        this.personality = personality;
        if (currentPet == null)
            return;

        currentPet.basic.personality = personality;    
    }

}
