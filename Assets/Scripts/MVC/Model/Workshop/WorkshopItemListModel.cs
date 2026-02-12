using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WorkshopItemListModel : SelectModel<ItemInfo>
{
    [SerializeField] private IInputField searchInputField;
    public string searchString => searchInputField?.inputString;
    public bool isIdFilter => int.TryParse(searchString, out _);
    public Func<ItemInfo, bool> searchFilter => GetSearchFilter();

    private string sortType = "id";
    private bool sortByDescendOrder = false;
    private Func<ItemInfo, float> sorter => GetSorter();

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

    private Func<ItemInfo, bool> GetSearchFilter() {
        if (string.IsNullOrEmpty(searchString))
            return (itemInfo) => true;

        if (int.TryParse(searchString, out var searchId))
            return (itemInfo) => (itemInfo != null) && (itemInfo.id == searchId);

        if (searchString.TryTrimParentheses(out _, '(', ')'))
            return Parser.ParseConditionFilter<ItemInfo>(searchString, (ids, itemInfo) => {
                if (itemInfo == null)
                    return new float[]{0, float.MinValue};

                if ((ids[0] == "type") && (ids[1].ToItemType() != ItemType.None))
                    return new float[]{itemInfo.GetItemInfoIdentifier(ids[0]), (int)ids[1].ToItemType()};

                return ids.Select(id => itemInfo.TryGetItemInfoIdentifier(id, out var value) ? value : Identifier.GetNumIdentifier(id)).ToArray();
            });

        return (itemInfo) => (itemInfo != null) && itemInfo.name.Contains(searchString);
    }

    private Func<ItemInfo, float> GetSorter() {
        return (itemInfo) => itemInfo?.GetItemInfoIdentifier(sortType) ?? float.MinValue;
    }
}
