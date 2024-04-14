using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PetElementView : Module
{
    [SerializeField] private PetElementButton titleElement;
    [SerializeField] private IText titleElementText;
    [SerializeField] private List<PetElementButton> weakArea;
    [SerializeField] private List<PetElementButton> resistArea;
    [SerializeField] private List<PetElementButton> zeroArea;

    public void SetElement(Element element) {
        titleElement?.SetElement(element);
        titleElementText?.SetText(element.ToString());

        var weakElements = PetElementSystem.GetAttackRelation(element, PetElementSystem.W);
        var resistElements = PetElementSystem.GetAttackRelation(element, PetElementSystem.R);
        var zeroElements = PetElementSystem.GetAttackRelation(element, PetElementSystem.O);
        SetElementRelation(weakArea, weakElements);
        SetElementRelation(resistArea, resistElements);
        SetElementRelation(zeroArea, zeroElements);
    }

    private void SetElementRelation(List<PetElementButton> area, List<Element> result) {
        for (int i = 0; i < area.Count; i++) {
            if (i >= result.Count) {
                area[i].SetActive(false);
                continue;
            }
            area[i].SetActive(true);
            area[i].SetElement(result[i]);
        }
    }

}
