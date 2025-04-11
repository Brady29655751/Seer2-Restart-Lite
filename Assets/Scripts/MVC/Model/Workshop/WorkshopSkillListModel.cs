using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WorkshopSkillListModel : Module
{
    [SerializeField] private IInputField searchInputField;
    public string searchString => searchInputField?.inputString;
    public bool isIdFilter => int.TryParse(searchString, out _);
    public Func<Skill, bool> searchFilter => GetSearchFilter();

    private string sortType = "id";
    private bool sortByDescendOrder = false;
    private Func<Skill, float> sorter => GetSorter();

    private List<Skill> skillDatabase = new List<Skill>();
    private IEnumerable<Skill> filteredSkillList => skillDatabase.Where(searchFilter);
    private IEnumerable<Skill> orderedSkillList => sortByDescendOrder ? filteredSkillList.OrderByDescending(sorter) : filteredSkillList.OrderBy(sorter);
    public List<Skill> resultSkillList => orderedSkillList.ToList();

    public void SetActive(bool active) {
        if (active)
            return;

        searchInputField.SetInputString(string.Empty);
    }

    public void SetSkillList(List<Skill> skillList) {
        skillDatabase = skillList;
    }

    public void Sort(string type) {
        if (type == sortType) {
            sortByDescendOrder = !sortByDescendOrder;
            return;
        }
        
        sortType = type;
        sortByDescendOrder = true;
    }

    private Func<Skill, bool> GetSearchFilter() {
        if (string.IsNullOrEmpty(searchString))
            return (skill) => true;

        if (int.TryParse(searchString, out var searchId))
            return (skill) => (skill != null) && (skill.id == searchId);

        if (searchString.TryTrimParentheses(out _, '(', ')'))
            return Parser.ParseConditionFilter<Skill>(searchString, (id, skill) => {
                if (skill == null)
                    return float.MinValue;

                return skill.TryGetSkillIdentifier(id, out var value) ? value : Identifier.GetNumIdentifier(id);
            });

        return (skill) => (skill != null) && skill.name.Contains(searchString);
    }

    private Func<Skill, float> GetSorter() {
        return (skill) => skill?.GetSkillIdentifier(sortType) ?? float.MinValue;
    }
}
