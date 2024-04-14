using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FilterController<T> : Module
{
    [SerializeField] private FilterModel<T> filterModel;

    public event Action<Func<T, bool>> onFilterEvent;

    public void Filter() {
        onFilterEvent?.Invoke(filterModel.filter);
    }
}
