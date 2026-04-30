using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PetDemoModel : Module
{
    public Pet currentPet { get; private set; }
    [SerializeField] public bool animMode = true;
    [SerializeField] private IInputField nameInputField;

    public string petName
    {
        get => currentPet?.name;
        set
        {
            currentPet.name = value;
            SaveSystem.SaveData();
        }
    }

    public string fixedName => nameInputField?.inputString;
    public Element element => currentPet.element;
    public Element subElement => currentPet.subElement;
    public Feature feature => currentPet.feature.feature;
    public Emblem emblem => currentPet.feature.emblem;
    public int iv => currentPet.talent.iv;
    public IVRanking ivRank => currentPet.talent.IVRank;

    private int kizunaDialogIndex = -1;


    public void SetPet(Pet pet)
    {
        currentPet = pet;
        nameInputField?.SetInputString(string.Empty);
        nameInputField?.SetPlaceHolderText(petName);
    }

    public void SetAnimMode(bool newMode)
    {
        animMode = newMode;
    }

    public void ChangePetName()
    {
        if (string.IsNullOrEmpty(fixedName))
        {
            Hintbox.OpenHintboxWithContent("精灵名称不能为空！", 16);
            return;
        }
        petName = fixedName;
    }

    public string Interact()
    {
        var candidates = currentPet?.info.kizuna?.GetDialogCandidates(currentPet?.kizuna ?? 0);
        if (ListHelper.IsNullOrEmpty(candidates))
            return null;

        if (candidates.Count == 1)
            return candidates.Get(0);
        
        while (true)
        {
            var index = Random.Range(0, candidates.Count);
            if (index != kizunaDialogIndex)
            {
                kizunaDialogIndex = index;
                return candidates.Get(index);   
            }
        }
    }    
}
