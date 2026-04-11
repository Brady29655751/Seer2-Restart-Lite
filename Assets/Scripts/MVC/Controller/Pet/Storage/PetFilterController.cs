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

    private void InitFilterSubscriptions()
    {
        if (elementController != null)
            elementController.onFilterEvent += Filter;
            elementController.onFilterEvent += f => { ResetNameController(); };

        if (nameController != null)
            nameController.onFilterEvent += Filter;
            nameController.onFilterEvent += f => { ResetElementController(); };
    }

    private void InitSortingSubscriptions()
    {
        if (sortingController != null)
            sortingController.onSortEvent += Sort;
    }

    public void Reset()
    {
        ResetElementController();
        ResetNameController();
    }

    private void ResetElementController()
    {
        elementController?.SetActive(false);
        elementController?.SetActive(true);
    }

    private void ResetNameController()
    {
        nameController?.SetActive(false);
    }

    public void Filter(Func<Pet, bool> filter)
    {
        selectController.Filter(filter);
    }

    public void Sort(Func<Pet, object> sorter)
    {
        selectController.Sort(sorter);
    }

}
