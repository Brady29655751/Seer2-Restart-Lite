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

        if (mode == PetBagMode.PVP) {
            var petList = GameManager.versionData.petData.petAllWithMod;
            var room = PhotonNetwork.CurrentRoom.CustomProperties;
            if (room == null)
                return petList;

            var buffList = (int[])(room["buff"]);
            if (buffList.Contains(Buff.BUFFID_PVP_IV_120))
                petList.ForEach(x => x.SetPetIdentifier("iv", 120));
                
            return petList;
        }
        return Player.instance.gameData.petStorage;
    }

    public void SetPetStorage(List<Pet> storage) {
        selectController.SetStorage(storage.ToList());
        selectController.Select(0);
    }
}
