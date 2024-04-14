using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PetPersonalityController : Module
{
    [SerializeField] private bool defaultSelect = true;
    [SerializeField] private PetPersonalityModel personalityModel;
    [SerializeField] private PetPersonalityView personalityView;

    public event Action<Personality> onSelectPersonalityEvent;

    public override void Init()
    {
        base.Init();
        if (defaultSelect)
            Select(0);
    }

    public void SetActive(bool active) {
        gameObject.SetActive(active);
    }

    public void SetPet(Pet pet) {
        personalityModel.SetPet(pet);
        SetPersonality(personalityModel.personality);
    }

    public void Select(int personalityId) {
        SetPersonality((Personality)personalityId);
        onSelectPersonalityEvent?.Invoke(personalityModel.personality);
    }

    public void SetPersonality(Personality personality) {
        personalityModel.SetPersonality(personality);
        personalityView.SetPersonalityText(personality);
        personalityView.SetNote(Status.GetPersonalityBuffDescription(personality));
        personalityView.SetRelationGraph(personality);
    }

}
