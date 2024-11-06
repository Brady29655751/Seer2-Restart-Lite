using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PetSelectHintbox : Hintbox
{
    [SerializeField] private PetSelectHintboxController panelController;
    [SerializeField] private PetSelectController selectController;

    public override void Init()
    {
        base.Init();
    }

    public override void SetPanelIdentifier(string id, string param) {
        switch (id) {
            default:
                base.SetPanelIdentifier(id, param);
                break;
            case "storage":
                SetStorage(param.Split('/').Select(p => {
                    var petInfo = p.ToIntList();
                    if (ListHelper.IsNullOrEmpty(petInfo))
                        return null;

                    if (petInfo.Count == 1)
                        return new Pet(petInfo[0], 1);

                    if (petInfo[1] < 0)
                        return Pet.GetExamplePet(petInfo[0], -petInfo[1]);

                    return new Pet(petInfo[0], petInfo[1]);
                }).ToList());
                break;
        }
    }

    public int[] GetCursor() => selectController?.GetCursor();

    public void SetStorage(List<Pet> storage) {
        selectController.SetStorage(storage);
        selectController.Select(0);
    }

    public void SetConfirmSelectCallback(Action<Pet> callback) {
        panelController.SetHintboxConfirmCallback(callback);
    }


}
