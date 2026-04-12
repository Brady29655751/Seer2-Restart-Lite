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
    [SerializeField] private PetStarFilterController starController;

    protected override void Awake()
    {
        base.Awake();
        InitFilterSubscriptions();
        InitSortingSubscriptions();
    }

    private void InitFilterSubscriptions()
    {
        if (elementController != null)
        {
            elementController.onFilterEvent += Filter;
            elementController.onFilterEvent += f => 
            { 
                ResetNameController(); 
                ResetStarController(); 
            };   
        }

        if (nameController != null)
        {
            nameController.onFilterEvent += Filter;
            nameController.onFilterEvent += f => 
            { 
                ResetElementController(); 
                ResetStarController(); 
            };   
        }

        if (starController != null)
        {
            starController.onFilterEvent += Filter;
            starController.onFilterEvent += f =>
            {
                ResetElementController();
                ResetNameController();
            };
        }
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

    private void ResetStarController()
    {
        starController?.SetActive(false);
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
