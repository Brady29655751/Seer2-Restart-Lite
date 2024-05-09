using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PetDictInfoView : Module
{
    [SerializeField] private InfoPrompt infoPrompt;
    [SerializeField] private Text idText;
    [SerializeField] private Text heightText;
    [SerializeField] private Text weightText;
    [SerializeField] private Text habitatText;
    [SerializeField] private Text descriptionText;
    [SerializeField] private List<IText> baseStatusTextList;
    [SerializeField] private IButton linkButton, testButton, editButton;
    [SerializeField] private PetBagPanel examplePetBagPanel;

    protected override void Awake() {
        examplePetBagPanel?.SetActive(true);
        examplePetBagPanel?.SetActive(false);
    }

    public void SelectMode(PetDictionaryMode mode) {
        testButton?.gameObject.SetActive(mode != PetDictionaryMode.Workshop);
        editButton?.gameObject.SetActive(mode == PetDictionaryMode.Workshop);
    }

    public void SetPetInfo(PetInfo info) {
        if (info == null) {
            Clear();
            return;
        }

        SetID(info.id);
        SetHeightAndWeight(info.basic.baseHeight, info.basic.baseWeight);
        SetHabitat(info.basic.habitat, info.basic.linkId);
        SetDescription(info.basic.description);
        SetBaseStatus(info.basic.baseStatus);
    }

    public void Clear() {
        idText?.SetText(string.Empty);
        heightText?.SetText(string.Empty);
        weightText?.SetText(string.Empty);
        habitatText?.SetText(string.Empty);
        descriptionText?.SetText(string.Empty);
        SetBaseStatus(Status.zero);

    }

    public void SetID(int id) {
        idText?.SetText(id.ToString());
    }

    public void SetHeightAndWeight(int baseHeight, int baseWeight) {
        heightText?.SetText(baseHeight + " - " + (baseHeight + 5) + " cm");
        weightText?.SetText(baseWeight + " - " + (baseWeight + 5) + " kg");
    }

    public void SetHabitat(string habitat, string linkId) {
        bool isLinkAvailable = !string.IsNullOrEmpty(linkId) && (linkId != "none");
        linkButton?.gameObject.SetActive(isLinkAvailable);
        habitatText?.gameObject.SetActive(!isLinkAvailable);
        habitatText?.SetText(habitat);
    }

    public void SetDescription(string description) {
        descriptionText?.SetText(description);
    }

    public void SetBaseStatus(Status baseStatus) {
        if (baseStatus == Status.zero) {
            foreach (var text in baseStatusTextList)
                text?.SetText(string.Empty);
            
            return;
        }
        for (int i = 0; i < 6; i++) {
            baseStatusTextList[i]?.SetText(((int)baseStatus[i]).ToString());
        }
    }

    public void SetInfoPromptActive(bool active) {
        infoPrompt.SetActive(active);
    }

    public void SetHabitatInfoPromptContent(string habitat) {
        infoPrompt.SetInfoPromptWithAutoSize(habitat, TextAnchor.MiddleLeft);
    } 

    public void OpenExamplePetBagPanel(int id) {
        examplePetBagPanel?.SetActive(true);
        examplePetBagPanel?.SetPetBag(new Pet[] { Pet.GetExamplePet(id) });
    }

}
