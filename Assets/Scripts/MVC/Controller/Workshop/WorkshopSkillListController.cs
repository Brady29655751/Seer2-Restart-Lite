using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WorkshopSkillListController : Module
{
    [SerializeField] private WorkshopSkillListModel skillListModel;
    [SerializeField] private WorkshopSkillListView skillListView;
    [SerializeField] private PageView pageView;
    [SerializeField] private WorkshopAllController allController;

    public override void Init()
    {
        base.Init();
        skillListModel.SetStorage(Skill.database);
        skillListView.CreateSkillList(Skill.database, OnEditSkill);
        OnSetPage();
    }

    private void OnEditSkill(Skill skill) {
        allController?.OnEditSkill(skill);
        SetActive(false);
    }

    public void SetActive(bool active) {
        skillListModel.SetActive(active);
        if (!active)
            OnSetPage();  
    }

    public void Search() 
    {
        skillListModel.Search();
        OnSetPage();
    }

    public void Sort(string key)
    {
        skillListModel.Sort(key);
        OnSetPage();
    }

    private void OnSetPage()
    {
        skillListView.SetSkillList(skillListModel.selections.ToList());
        pageView.SetPage(skillListModel.page, skillListModel.lastPage);
    }

    public void PrevPage()
    {
        skillListModel.PrevPage();
        OnSetPage();
    }

    public void NextPage()
    {
        skillListModel.NextPage();
        OnSetPage();
    }
}
