using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PetSortingController : Module
{
    [SerializeField] private PetSortingModel sortingModel;

    public Action<Func<Pet, object>> onSortEvent;

    public void Sort() {
        onSortEvent?.Invoke(sortingModel.sorter);
    }
} 
