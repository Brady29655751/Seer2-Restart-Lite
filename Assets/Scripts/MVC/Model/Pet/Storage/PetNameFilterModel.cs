using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PetNameFilterModel : FilterModel<Pet>
{
    public override Func<Pet, bool> GetFilter() {
        if (!isWorking)
            return (x) => true;
        
        FilterType filterType = type;
        string input = inputString;

        switch (filterType) {
            default:
            case FilterType.Name:
                return x => x.name.Contains(input);
            case FilterType.ID:
                int id = int.Parse(input);
                return x => x.id == id;
        }
    }
}
