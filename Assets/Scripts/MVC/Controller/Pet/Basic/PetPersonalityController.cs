using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PetPersonalityController : Module
{
    private List<Personality> personalityList => Enumerable.Range(0, Enum.GetNames(typeof(Personality)).Length).Select(x => (Personality)x).ToList();

    [SerializeField] private PetPersonalityModel personalityModel;
    [SerializeField] private PetPersonalityView personalityView;

    public event Action<Personality> onSelectPersonalityEvent;
    private Action onInitCallback;

    public override void Init()
    {
        base.Init();
        personalityView.CreatePersonalityList(personalityList, (p) => Select((int)p), onInitCallback);
    }

    public void SetActive(bool active, Action onInitCallback = null) {
        this.onInitCallback = onInitCallback;
        gameObject.SetActive(active);
    }

    public void SetPet(Pet pet) {
        personalityModel.SetPet(pet);
        SetPersonality(personalityModel.personality);
    }

    public void Filter(Func<Personality, bool> filter) {
        personalityView.SetPersonalityList(personalityList.Where(filter).ToList());
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
