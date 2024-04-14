using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IFilter<T>
{
    private static Func<T, int, bool> defaultFilter => ((x, i) => true);
    private bool sortByDesc;
    private Func<T, object> sorter = null;
    private Func<T, int, bool> filter = defaultFilter;

    public void Reset() {
        SetSortingOptions();
        SetFilterOptions();
    }

    public void SetSortingOptions(bool desc = true, Func<T, object> sorter = null) {
        this.sortByDesc = desc;
        this.sorter = sorter;
    }

    public void SetSortingOptions<TKey>(bool desc = true, Func<T, TKey> sorter = null) {
        this.sortByDesc = desc;
        if (sorter == null)
            this.sorter = null;
        else
            this.sorter = (x) => (sorter.Invoke(x));
    }

    public void SetFilterOptions(Func<T, bool> filter) {
        Func<T, int, bool> newFilter = null;
        if (filter != null)
            newFilter = (x, i) => filter.Invoke(x);
    
        SetFilterOptions(newFilter);
    }

    public void SetFilterOptions(Func<T, int, bool> filter = null) {
        this.filter = filter ?? defaultFilter;
    }

    public IEnumerable<T> Filter(IEnumerable<T> source) {
        return source.Where(filter);
    }

    public IEnumerable<T> Sort(IEnumerable<T> source) {
        if (sorter == null)
            return source;
        
        if (sortByDesc)
            return source.OrderByDescending(sorter);

        return source.OrderBy(sorter);
    }
    
}
