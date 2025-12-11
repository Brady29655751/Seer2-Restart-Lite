using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PetDictionaryController : Module
{
    public VersionPetData petData => GameManager.versionData.petData;
    public List<Pet> petDictionary => petData.petAllWithMod.Where(x => !x.info.ui.hide).OrderByDescending(PetSortingModel.GetSorter(PetSortingOptions.PositiveId)).ToList();
    public List<Pet> petTopic => petData.petTopic;
    public List<Pet> petWorkshop = new List<Pet>();

    [SerializeField] private PetDictionaryMode mode = PetDictionaryMode.Topic;
    [SerializeField] private PetSelectController selectController;
    [SerializeField] private PetBagPanel petBagPanel;

    protected override void Awake()
    {
        base.Awake();
        selectController.onSetSelectionsEvent += onSetSelections;
    }

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
        bool isWorkshop = (mode == PetDictionaryMode.Workshop) && (ListHelper.IsNullOrEmpty(petWorkshop));
        if (isWorkshop)
            petWorkshop = storage.Where(x => !x.info.ui.hide).ToList();

        var petStorage = (mode == PetDictionaryMode.Workshop) ? petWorkshop : storage.ToList();
        selectController.SetStorage(petStorage);
        selectController.Select(0);
    }

    public void TestBattle(int mapId) {
        Map.TestBattle(mapId, petBagPanel?.petBag);
    }

    public void onSetSelections()
    {
        var getMarks = selectController.GetPetSelections().Select(x => (x != null) && x.GetPetIdentifier("own") > 0).ToList();
        selectController.SetGetMarks(getMarks);
    }
}

public enum PetDictionaryMode {
    All = 0,
    Topic = 1,
    Workshop = 2,
}
