using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PetSortingModel : Module
{
    [SerializeField] private PetSortingType sortingType = PetSortingType.Storage;
    [SerializeField] private IDropdown dropdown;
    public bool isWorking { get; protected set; } = true;
    public PetSortingOptions sortingOption => GetSortingOptions();
    public Func<Pet, object> sorter => GetSorter();

    public PetSortingOptions GetSortingOptions() {
        if (dropdown == null)
            return PetSortingOptions.None;
        
        return sortingType switch {
            PetSortingType.Storage => (PetSortingOptions)(dropdown.value + 1),
            PetSortingType.Dict => (PetSortingOptions)(-dropdown.value - 1),
            _ => PetSortingOptions.None
        };
    }

    public Func<Pet, object> GetSorter() {
        if (!isWorking)
            return null;

        switch (sortingOption) {
            default:
            case PetSortingOptions.PositiveId:
                return (Pet x) => (x.id > 0) ? (int.MaxValue - x.id) : (x.id);
            case PetSortingOptions.NegativeId:
                return (Pet x) => (x.id > 0) ? (-x.id) : (int.MaxValue + x.id);
            case PetSortingOptions.Date:
                return (Pet x) => x.basic.getPetDate;
            case PetSortingOptions.Level:
                return (Pet x) => x.level;
        }
    }
}

public enum PetSortingOptions {
    NegativeId = -2,
    PositiveId = -1,
    None = 0,
    Date = 1,
    Level = 2,
}

public enum PetSortingType {
    Storage = 0,
    Dict = 1,
}
