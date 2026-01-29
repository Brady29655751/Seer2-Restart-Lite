using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PetElementModel : Module
{
    [SerializeField] private int elementNumInOnePage = 23;
    public int page { get; private set; }= 0;
    public int lastPage => (PetElementSystem.elementNum - 1) / elementNumInOnePage;
    public Element element { get; private set; }

    public int GetElementId(int index) => page * elementNumInOnePage + index;

    public void SetElement(Element element) {
        this.element = element;
    }

    public void PrevPage() {
        page = Mathf.Max(0, page - 1);
    }

    public void NextPage() {
        page = Mathf.Min(page + 1, lastPage);
    }
}
