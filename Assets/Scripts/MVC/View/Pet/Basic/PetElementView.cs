using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PetElementView : Module
{
    [SerializeField] private bool isAttackRelation = true;
    [SerializeField] private PetElementFilterButton titleElement;
    [SerializeField] private IText titleElementText;
    [SerializeField] private List<PetElementFilterButton> selectArea;
    [SerializeField] private List<PetElementFilterButton> weakArea;
    [SerializeField] private List<PetElementFilterButton> resistArea;
    [SerializeField] private List<PetElementFilterButton> zeroArea;

    public override void Init() {
        // SetPage(0);
    }

    public void SetPage(int page) {
        var offset = page * selectArea.Count;
        for (int i = 0; i < selectArea.Count; i++) {
            var elementId = offset + i;
            selectArea[i].gameObject.SetActive(elementId < PetElementSystem.elementNum);
            if (elementId >= PetElementSystem.elementNum)
                continue;

            selectArea[i].SetElement((Element)elementId);
        }
    }

    public void SetElement(Element element) {
        titleElement?.SetElement(element);
        titleElementText?.SetText(element.GetElementName());

        Func<Element, Func<float, bool>, List<Element>> RelationFunc = isAttackRelation
            ? PetElementSystem.GetAttackRelation
            : PetElementSystem.GetDefenseRelation;

        var weakElements = RelationFunc(element, x => x > 1);
        var resistElements = RelationFunc(element, x => (x > 0) && (x < 1));
        var zeroElements = RelationFunc(element, x => x == 0);
        
        SetElementRelation(weakArea, weakElements);
        SetElementRelation(resistArea, resistElements);
        SetElementRelation(zeroArea, zeroElements);
    }

    private void SetElementRelation(List<PetElementFilterButton> area, List<Element> result) {
        for (int i = 0; i < area.Count; i++) {
            if (i >= result.Count) {
                area[i].gameObject.SetActive(false);
                continue;
            }
            area[i].gameObject.SetActive(true);
            area[i].SetElement(result[i]);
        }
    }

}
