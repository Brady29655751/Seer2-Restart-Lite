using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PetElementFilterButton : IButton
{
    [SerializeField] private Image elementIcon;

    public void SetElement(Element element) {
        elementIcon.SetElementSprite(element);
    }
}
