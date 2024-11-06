using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PetDictionaryController : Module
{
    public VersionPetData petData => GameManager.versionData.petData;
    public List<Pet> petDictionary => petData.petDictionary;
    public List<Pet> petTopic => petData.petTopic;
    public List<Pet> petWorkshop = new List<Pet>();

    [SerializeField] private PetDictionaryMode mode = PetDictionaryMode.Topic;
    [SerializeField] private PetSelectController selectController;

    public override void Init()
    {
        base.Init();
        SelectMode((int)mode);
    }

    public PetDictionaryMode GetMode() => mode;

    public List<Pet> GetStorage() {
        return mode switch {
            PetDictionaryMode.All => petDictionary,
            PetDictionaryMode.Topic => petTopic,
            PetDictionaryMode.Workshop => petWorkshop,
            _ => new List<Pet>(),
        };
    }

    public void SelectMode(int modeId) {
        if (mode == PetDictionaryMode.Workshop)
            return;
            
        mode = (PetDictionaryMode)modeId;
        SetPetStorage(GetStorage());
    }

    public void SetPetStorage(List<Pet> storage) {
        if ((mode == PetDictionaryMode.Workshop) && (ListHelper.IsNullOrEmpty(petWorkshop)))
            petWorkshop = storage.Where(x => !x.info.ui.hide).ToList();

        var petStorage = (mode == PetDictionaryMode.Workshop) ? petWorkshop : storage.ToList();
        selectController.SetStorage(petStorage);
        selectController.Select(0);
    }
}

public enum PetDictionaryMode {
    All = 0,
    Topic = 1,
    Workshop = 2,
}
