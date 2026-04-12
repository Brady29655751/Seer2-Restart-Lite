using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class PetStoragePanel : Panel
{
    private List<Pet> petStorage => GetDefaultPetStorage(mode);
    [SerializeField] private PetBagMode mode = PetBagMode.Normal;
    [SerializeField] private PetSelectController selectController;
    [SerializeField] private PetDemoController demoController;
    [SerializeField] private PetDetailController detailController;
    [SerializeField] private PetFeatureController featureController;

    protected override void Awake()
    {
        base.Awake();
        InitSelectSubscriptions();
    }

    public override void Init()
    {
        base.Init();
        SetPetStorage(petStorage);
        selectController.Sort(x => (x == null) ? DateTime.MinValue : x.basic.getPetDate);
    }

    private void InitSelectSubscriptions()
    {
        selectController.onSelectPetEvent += demoController.SetPet;
        selectController.onSelectPetEvent += detailController.SetPet;
        selectController.onSelectPetEvent += featureController.SetPet;
    }

    public static bool IsNormalStorageMode(PetBagMode mode)
    {
        var okList = new PetBagMode[]{ PetBagMode.Normal, PetBagMode.Elite, PetBagMode.Mod };
        return okList.Contains(mode);
    }

    public static List<Pet> GetDefaultPetStorage(PetBagMode mode)
    {

        if (mode == PetBagMode.PVP)
        {
            var petList = GameManager.versionData.petData.petAllWithMod.Where(x => !x.info.ui.hide).ToList();
            var room = PhotonNetwork.CurrentRoom.CustomProperties;
            if (room == null)
                return petList;

            var buffList = (int[])(room["buff"]);
            if (buffList.Contains(Buff.BUFFID_PVP_IV_120))
                petList.ForEach(x => x.SetPetIdentifier("iv", 120));

            return petList;
        }

        var petStorage = Player.instance.gameData.petStorage.Where(x => x != null);
        petStorage = mode switch 
        {
            PetBagMode.Normal => petStorage.Where(x => !PetInfo.IsMod(x.id) && !x.record.GetRecord("elite", false)),
            PetBagMode.Mod => petStorage.Where(x => PetInfo.IsMod(x.id) && !x.record.GetRecord("elite", false)),
            PetBagMode.Elite => petStorage.Where(x => x.record.GetRecord("elite", false)),
            _ => petStorage,
        };
        return petStorage.ToList();
    }

    public void SetPetStorage(List<Pet> storage)
    {
        selectController.SetStorage(storage.ToList());
        selectController.Select(0);
    }

    public void SetMode(int mode)
    {
        if (this.mode == PetBagMode.PVP)
            return;
        
        this.mode = (PetBagMode)mode;
        demoController.SetMode(this.mode);
        featureController.SetMode(this.mode);
    }
}
