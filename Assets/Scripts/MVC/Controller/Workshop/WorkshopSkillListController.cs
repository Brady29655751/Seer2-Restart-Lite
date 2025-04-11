using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WorkshopSkillListController : Module
{
    [SerializeField] private WorkshopSkillListModel skillListModel;
    [SerializeField] private WorkshopSkillListView skillListView;
    [SerializeField] private WorkshopAllController allController;

    public override void Init()
    {
        base.Init();
        skillListModel.SetSkillList(Skill.database);
        skillListView.CreateSkillList(Skill.database, OnEditSkill);
    }

    private void OnEditSkill(Skill skill) {
        allController?.OnEditSkill(skill);
        SetActive(false);
    }

    public void SetActive(bool active) {
        skillListModel.SetActive(active);
        if (!active) 
            skillListView.SetSkillList(Skill.database);
    }

    public void Search() {
        skillListView.ShowResult(skillListModel.resultSkillList, skillListModel.isIdFilter);
    }

    public void Sort(string type) {
        skillListModel.Sort(type);
        skillListView.ShowResult(skillListModel.resultSkillList, false);
    }
}
