using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PetFilterController : Module
{
    [SerializeField] private PetSelectController selectController;
    [SerializeField] private PetSortingController sortingController;
    [SerializeField] private PetElementFilterController elementController;
    [SerializeField] private PetNameFilterController nameController;

    protected override void Awake()
    {
        base.Awake();
        InitFilterSubscriptions();
        InitSortingSubscriptions();
    }

    private void InitFilterSubscriptions() {
        if (elementController != null)
            elementController.onFilterEvent += Filter;
        if (nameController != null)
            nameController.onFilterEvent += Filter;
    }

    private void InitSortingSubscriptions() {
        if (sortingController != null)
            sortingController.onSortEvent += Sort;
    }

    public void Filter(Func<Pet, bool> filter) {
        selectController.Filter(filter);
    }

    public void Sort(Func<Pet, object> sorter) {
        selectController.Sort(sorter);
    }

}
