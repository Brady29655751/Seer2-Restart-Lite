using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PetElementFilterModel : Module
{
    public bool isWorking { get; protected set; } = true;
    public Element element { get; protected set; } = Element.全部;
    public Func<Pet, bool> filter => GetFilter();

    public void OnControlButtonClick() {
        SetActive(!isWorking);
    }

    public void SetActive(bool active) {
        isWorking = active;
        element = Element.全部;
    }

    public bool SetElement(int index) {
        if (!index.IsInRange(0, PetElementSystem.elementNum))
            return false;
        
        Element newElement = (Element)index;
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
