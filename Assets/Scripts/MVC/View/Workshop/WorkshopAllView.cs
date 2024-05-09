using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorkshopAllView : Module
{
    [SerializeField] private List<GameObject> createModObjectList, checkModObjectList;
    [SerializeField] private Panel allSkillPanel, allBuffPanel;

    public void NeverCreateMod() {
        createModObjectList.ForEach(x => x.SetActive(true));
        checkModObjectList.ForEach(x => x.SetActive(false));
    }

    public void CheckCurrentMod() {
        createModObjectList.ForEach(x => x.SetActive(false));
        checkModObjectList.ForEach(x => x.SetActive(true));
    }
    
    public void OpenAllSkillPanel() {
        allSkillPanel.SetActive(true);
    }
    
    public void OpenAllBuffPanel() {
        allBuffPanel.SetActive(true);
    }
    
    
}
