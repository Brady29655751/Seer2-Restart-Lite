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
        if (!PetElementSystem.IsMod())
            return;

        for (int i = 0; i < selectArea.Count; i++) {
            selectArea[i].gameObject.SetActive(i < PetElementSystem.elementNum);
            if (i >= PetElementSystem.elementNum)
                continue;

            selectArea[i].SetElement((Element)i);
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
