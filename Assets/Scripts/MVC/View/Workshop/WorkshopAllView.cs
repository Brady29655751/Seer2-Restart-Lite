using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorkshopAllView : Module
{
    [SerializeField] private List<GameObject> createModObjectList, checkModObjectList;
    [SerializeField] private Panel allSkillPanel, allBuffPanel, allItemPanel, skillListPanel, itemListPanel;

    public void NeverCreateMod() {
        createModObjectList.ForEach(x => x.SetActive(true));
        checkModObjectList.ForEach(x => x.SetActive(false));
    }

    public void CheckCurrentMod() {
        createModObjectList.ForEach(x => x.SetActive(false));
        checkModObjectList.ForEach(x => x.SetActive(true));
    }
    
    public void SetAllSkillPanelActive(bool active) {
        allSkillPanel.SetActive(active);
    }

    public void SetAllBuffPanelActive(bool active) {
        allBuffPanel.SetActive(active);
    }

    public void SetAllItemPanelActive(bool active) {
        allItemPanel.SetActive(active);
    }
    
    public void SetSkillListPanelActive(bool active) {
        skillListPanel.SetActive(active);
    }

    public void SetItemListPanelActive(bool active) {
        itemListPanel.SetActive(active);
    }
}
