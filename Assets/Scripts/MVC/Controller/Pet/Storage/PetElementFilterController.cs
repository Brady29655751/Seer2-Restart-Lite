using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PetElementFilterController : Module
{
    [SerializeField] private PetElementFilterModel filterModel;
    [SerializeField] private PetElementFilterView filterView;

    public Action<Func<Pet, bool>> onFilterEvent;

    public override void Init()
    {
        base.Init();
        SetActive(true);
    }

    public void SetActive(bool active) {
        filterModel.SetActive(active);
        filterView.SetActive(active);
    }

    public void OnControlButtonClick() {
        filterModel.OnControlButtonClick();
        filterView.OnControlButtonClick();
        Filter();
    }

    public void Select(int index) {
        bool success = filterModel.SetElement(index);
        if (!success)
            return;

        Filter();
    }

    public void Filter() {
        onFilterEvent?.Invoke(filterModel.filter);
    }

    public void SetInfoPromptActive(bool active) {
        filterView.SetInfoPromptActive(active);
    }

    public void SetElementInfo(int index) {
        var elementList = PetElementSystem.elementList;
        if (!index.IsInRange(0, elementList.Count))
            return;

        filterView.SetElementInfo(elementList[index]);
    }
}
