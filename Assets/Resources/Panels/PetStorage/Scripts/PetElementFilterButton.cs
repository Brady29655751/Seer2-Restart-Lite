using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PetElementButton : IButton
{
    [SerializeField] private Image elementIcon;

    public void SetActive(bool active) {
        gameObject.SetActive(active);
    }

    public void SetElement(Element element) {
        elementIcon.SetElementSprite(element);
    }
}
