using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WorkshopSkillListModel : SelectModel<Skill>
{
    [SerializeField] private IInputField searchInputField;
    public string searchString => searchInputField?.inputString;
    public bool isIdFilter => int.TryParse(searchString, out _);
    public Func<Skill, bool> searchFilter => GetSearchFilter();

    private string sortType = "id";
    private bool sortByDescendOrder = false;
    private Func<Skill, float> sorter => GetSorter();


    public void SetActive(bool active) {
        if (active)
            return;

        searchInputField.SetInputString(string.Empty);
        Reset();
    }

    public void Search()
    {
        Filter(searchFilter);
        Sort(sorter, sortByDescendOrder);
    }

    public void Sort(string type)
    {
        if (type == sortType) 
        {
            sortByDescendOrder = !sortByDescendOrder;
        } 
        else
        {
            sortType = type;
            sortByDescendOrder = true;   
        }

        Search();
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
