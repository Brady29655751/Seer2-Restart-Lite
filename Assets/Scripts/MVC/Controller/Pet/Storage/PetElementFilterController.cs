using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This component is in 3 places: Pet Dictionary, Pet Storage, and PVP Room.
public class PetElementFilterController : Module
{
    [SerializeField] private PetElementFilterModel filterModel;
    [SerializeField] private PetElementFilterView filterView;
    [SerializeField] private PageView pageView;

    public Action<Func<Pet, bool>> onFilterEvent;

    public override void Init()
    {
        base.Init();
        SetActive(true);
        OnSetPage();
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

    public void OnSetPage() {
        filterView.SetPage(filterModel.page);
        pageView?.SetPage(filterModel.page, filterModel.lastPage);
    }

    public void PrevPage() {
        filterModel.PrevPage();
        OnSetPage();
    }

    public void NextPage() {
        filterModel.NextPage();
        OnSetPage();
    }

    public void SetInfoPromptActive(bool active) {
        filterView.SetInfoPromptActive(active);
    }

    public void SetElementInfo(int index) {
        var elementId = filterModel.GetElementId(index);
        if (!elementId.IsInRange(0, PetElementSystem.elementNum))
            return;

        filterView.SetElementInfo((Element)elementId);
    }
}
