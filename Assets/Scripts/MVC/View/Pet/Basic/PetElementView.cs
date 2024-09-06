using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PetElementView : Module
{
    [SerializeField] private PetElementButton titleElement;
    [SerializeField] private IText titleElementText;
    [SerializeField] private List<PetElementButton> selectArea;
    [SerializeField] private List<PetElementButton> weakArea;
    [SerializeField] private List<PetElementButton> resistArea;
    [SerializeField] private List<PetElementButton> zeroArea;

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

        var weakElements = PetElementSystem.GetAttackRelation(element, x => x > 1);
        var resistElements = PetElementSystem.GetAttackRelation(element, x => (x > 0) && (x < 1));
        var zeroElements = PetElementSystem.GetAttackRelation(element, x => x == 0);
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
