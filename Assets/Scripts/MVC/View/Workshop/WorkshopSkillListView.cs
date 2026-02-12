using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class WorkshopSkillListView : UIModule
{
    [SerializeField] private List<SkillCell> skillCellList = new List<SkillCell>();

    public void CreateSkillList(List<Skill> skills, Action<Skill> callback = null) {
        for (int i = 0; i < skillCellList.Count; i++)
        {
            var cell = skillCellList[i];
            cell.SetInfoPrompt(infoPrompt);   
            cell.SetSkill(skills.Get(i));
            cell.SetCallback(callback, "id");
        }
    }

    public void SetSkillList(List<Skill> skills) {
        for (int i = 0; i < skillCellList.Count; i++)
            skillCellList[i].SetSkill(skills.Get(i));
    }
}
