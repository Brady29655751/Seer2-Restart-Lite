using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PetSelectModel : SelectModel<Pet>
{

    protected override void SetSelections(Pet[] selections)
    {
        Array.Resize(ref selections, capacity);
        base.SetSelections(selections);
    }
}
