using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PetDictInfoModel : Module
{
    public PetInfo petInfo { get; private set; }

    public void SetPetId(int id) {
        petInfo = Database.instance.GetPetInfo(id);
    }
}
