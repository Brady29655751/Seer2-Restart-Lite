using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PetElementController : Module
{
    [SerializeField] private PetElementModel elementModel;
    [SerializeField] private PetElementView elementView;
    [SerializeField] private PageView pageView;

    public override void Init()
    {
        base.Init();
        OnSetPage();
    }

    public void Select(int index) {
        int elementId = elementModel.GetElementId(index);
        SetElement((Element)elementId);
    }

    public void SetElement(Element element) {
        elementModel.SetElement(element);
        elementView.SetElement(element);
    }

    public void OnSetPage() {
        elementView.SetPage(elementModel.page);
        pageView.SetPage(elementModel.page, elementModel.lastPage);
        Select(0);
    }

    public void PrevPage() {
        elementModel.PrevPage();
        OnSetPage();
    }

    public void NextPage() {
        elementModel.NextPage();
        OnSetPage();
    }
}
