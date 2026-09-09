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

    public PetKizunaDialog kizunaDialogInfo;
    public int kizunaDialogIndex = -1;


    public void SetPet(Pet pet)
    {
        currentPet = pet;
        nameInputField?.SetInputString(string.Empty);
        nameInputField?.SetPlaceHolderText(petName);

        kizunaDialogInfo = currentPet?.info.kizuna?.GetKizunaDialog(currentPet?.kizuna ?? 0);
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

    public void Interact(out string content, out AudioClip sound)
    {
        content = null;
        sound = null;

        var candidateContents = kizunaDialogInfo?.contents;
        var candidateSounds = kizunaDialogInfo?.sounds;

        if (ListHelper.IsNullOrEmpty(candidateContents))
        {
            kizunaDialogIndex = -1;
            return;
        }
            
        if (candidateContents.Count == 1)
        {
            kizunaDialogIndex = 0;
            content = candidateContents?.Get(0);
            sound = candidateSounds?.Get(0);
            return;
        }
        
        while (true)
        {
            var index = Random.Range(0, candidateContents.Count);
            if (index != kizunaDialogIndex)
            {
                kizunaDialogIndex = index;
                content = candidateContents?.Get(index);
                sound = candidateSounds?.Get(index);
                return;
            }
        }
    }
}
