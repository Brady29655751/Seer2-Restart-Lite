using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FilterController<T> : Module
{
    [SerializeField] protected FilterModel<T> filterModel;

    public event Action<Func<T, bool>> onFilterEvent;

    public void SetActive(bool active)
    {
        filterModel.SetActive(active);
    }

    public void Filter()
    {
        onFilterEvent?.Invoke(filterModel.filter);
    }
}
