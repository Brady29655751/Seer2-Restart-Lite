using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PetElementFilterModel : Module
{
    [SerializeField] private int elementNumInOnePage = 20;
    public int page { get; private set; } = 0;
    public int lastPage => (PetElementSystem.elementNum - 1) / elementNumInOnePage;
    public bool isWorking { get; protected set; } = true;
    public Element element { get; protected set; } = Element.全部;
    public Func<Pet, bool> filter => GetFilter();

    public int GetElementId(int index) => page * elementNumInOnePage + index;

    public void OnControlButtonClick() {
        SetActive(!isWorking);
    }

    public void SetActive(bool active) {
        isWorking = active;
        element = Element.全部;
    }

    public void PrevPage() {
        page = Mathf.Max(0, page - 1);
    }

    public void NextPage() {
        page = Mathf.Min(page + 1, lastPage);
    }

    public bool SetElement(int index) {
        var elementId = GetElementId(index);
        if (!elementId.IsInRange(0, PetElementSystem.elementNum))
            return false;
        
        Element newElement = (Element)elementId;
        if (element == newElement)
            return false;

        element = newElement;
        return true;
    }

    public Func<Pet, bool> GetFilter() {
        if (!isWorking)
            return (x) => true;

        Element element = this.element;
        if (element == Element.全部)
            return (x) => true;

        return (x) => ((x.element == element) || ((x.subElement == element) && (x.subElement != Element.普通)));
    }

}
