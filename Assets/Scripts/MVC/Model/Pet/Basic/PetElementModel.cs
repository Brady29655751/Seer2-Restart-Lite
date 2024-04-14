using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PetElementModel : Module
{
    public Element element { get; private set; }

    public void SetElement(Element element) {
        this.element = element;
    }
}
