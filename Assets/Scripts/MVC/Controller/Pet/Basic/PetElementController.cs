using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PetElementController : Module
{
    [SerializeField] private PetElementModel elementModel;
    [SerializeField] private PetElementView elementView;

    public override void Init()
    {
        base.Init();
        Select(0);
    }

    public void Select(int elementId) {
        SetElement((Element)elementId);
    }

    public void SetElement(Element element) {
        elementModel.SetElement(element);
        elementView.SetElement(element);
    }
}
