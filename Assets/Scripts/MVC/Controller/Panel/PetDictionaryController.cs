using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PetDictionaryController : Module
{
    public VersionPetData petData => GameManager.versionData.petData;
    public List<Pet> petDictionary => petData.petDictionary;
    public List<Pet> petTopic => petData.petTopic;

    [SerializeField] private PetDictionaryMode mode = PetDictionaryMode.Topic;
    [SerializeField] private PetSelectController selectController;

    public override void Init()
    {
        base.Init();
        SetPetStorage(petTopic);
    }

    public void SelectMode(int modeId) {
        mode = (PetDictionaryMode)modeId;
        SetPetStorage((mode == PetDictionaryMode.All) ? petDictionary : petTopic);
    }

    public void SetPetStorage(List<Pet> storage) {
        selectController.SetStorage(storage.ToList());
        selectController.Select(0);
    }
}

public enum PetDictionaryMode {
    All = 0,
    Topic = 1,
}
