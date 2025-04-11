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
            PetSortingType.Dict => (PetSortingOptions)((dropdown.value + 1) * Mathf.Sign(dropdown.value - 2)),
            _ => PetSortingOptions.None
        };
    }

    public Func<Pet, object> GetSorter() {
        if (!isWorking)
            return null;

        return GetSorter(sortingOption);
    }

    public static Func<Pet, object> GetSorter(PetSortingOptions option) {
        switch (option) {
            default:
            case PetSortingOptions.PositiveId:
                return (Pet x) => (x.GetPetIdentifier("id") > 0) ? (int.MaxValue - x.GetPetIdentifier("id"), -x.GetPetIdentifier("subId")) : (x.GetPetIdentifier("id"), -x.GetPetIdentifier("subId"));
            case PetSortingOptions.NegativeId:
                return (Pet x) => (x.GetPetIdentifier("id") > 0) ? (-x.GetPetIdentifier("id"), -x.GetPetIdentifier("subId")) : (int.MaxValue + x.GetPetIdentifier("id"), -x.GetPetIdentifier("subId"));
            case PetSortingOptions.Date:
                return (Pet x) => x.basic.getPetDate;
            case PetSortingOptions.Level:
                return (Pet x) => x.level;
            case PetSortingOptions.Star:
                return (Pet x) => x.info.star;
        }
    }
}

public enum PetSortingOptions {
    NegativeId = -2,
    PositiveId = -1,
    None = 0,
    Date = 1,
    Level = 2,
    Star = 3,
}

public enum PetSortingType {
    Storage = 0,
    Dict = 1,
}
