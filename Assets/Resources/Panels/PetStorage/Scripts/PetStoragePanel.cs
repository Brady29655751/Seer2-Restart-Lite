using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PetStoragePanel : Panel
{
    private List<Pet> petStorage => GetDefaultPetStorage(mode);
    [SerializeField] private PetBagMode mode = PetBagMode.Normal;
    [SerializeField] private PetSelectController selectController;
    [SerializeField] private PetDemoController demoController;
    [SerializeField] private PetDetailController detailController;

    protected override void Awake()
    {
        base.Awake();
        InitSelectSubscriptions();
    }

    public override void Init()
    {
        base.Init();
        SetPetStorage(petStorage);
    }

    private void InitSelectSubscriptions() {
        selectController.onSelectPetEvent += demoController.SetPet;
        selectController.onSelectPetEvent += detailController.SetPet;
    }

    public static List<Pet> GetDefaultPetStorage(PetBagMode mode) {
        return mode switch {
            PetBagMode.PVP  => GameManager.versionData.petData.petAllWithMod,
            _               => Player.instance.gameData.petStorage,
        };
    }

    public void SetPetStorage(List<Pet> storage) {
        selectController.SetStorage(storage.ToList());
        selectController.Select(0);
    }
}
