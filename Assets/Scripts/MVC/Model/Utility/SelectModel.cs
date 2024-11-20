using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectModel<T> : Module, IPageHandler
{
    public int page { get; protected set; }
    public int lastPage { get => GetLastPage(); }

    [SerializeField] protected int selectCount = 1;
    [SerializeField] protected int capacity = 4;
    [SerializeField] protected PageMode pageMode = PageMode.NewPage;
    [SerializeField] protected List<T> storage = new List<T>();
    protected List<T> sortedStorage => filter.Sort(storage).ToList();
    protected List<T> resultStorage => filter.Filter(sortedStorage).ToList();
    protected IFilter<T> filter = new IFilter<T>();
    

    protected ISelectableArray<T> selectableArray = new ISelectableArray<T>(4);
    public bool[] isSelected => selectableArray.isSelected;
    public T[] selections => selectableArray.items;
    public T[] currentSelectedItems => selectableArray.currentSelectItems;
    public int[] cursor => selectableArray.currentSelectIndex;
    public int count => resultStorage.Count;
    public int selectionSize => selectableArray.size;
    public int selectionCapacity => capacity;


    protected override void Awake() {
        base.Awake();
        selectableArray.SetCapacity(capacity);
        selectableArray.SetSelectCount(selectCount);
        SetPage(0);
    }

    public virtual int GetLastPage() {
        switch (pageMode) {
            default:
            case PageMode.NewPage:
                return (resultStorage.Count - 1) / selectableArray.capacity;
            case PageMode.Indent:
                return Mathf.Max(0, resultStorage.Count - selectableArray.capacity);
        }
    }

    public virtual void SetPage(int newPage) {
        page = Mathf.Clamp(newPage, 0, lastPage);
        T[] newSelections = resultStorage.Where((x, i) => PageFilter(i)).ToArray();
        SetSelections(newSelections);
    }

    protected virtual bool PageFilter(int index) {
        return index.IsInRange(GetResultStorageIndex(0), GetResultStorageIndex(selectableArray.capacity));
    }

    protected virtual int GetResultStorageIndex(int selectIndex) {
        switch (pageMode) {
            default:
            case PageMode.NewPage:
                return page * selectableArray.capacity + selectIndex;
            case PageMode.Indent:
                return page + selectIndex;
        }
    }

    public virtual void PrevPage() {
        SetPage(page - 1);
    }

    public virtual void NextPage() {
        SetPage(page + 1);
    }

    public virtual void SetStorage(List<T> storage, int defaultSelectPage = 0) {
        this.storage = storage;
        Reset(defaultSelectPage);
    }

    protected virtual void SetSelections(T[] selections) {
        selectableArray.SetArray(selections, selections == this.selections);
    }

    protected virtual void SetSelectionsCapacity(int capacity) {
        selectableArray.SetCapacity(capacity);
    }


    public virtual void Reset(int defaultSelectPage = 0) {
        filter.Reset();
        SetPage(defaultSelectPage);
    }

    public virtual void Select(int index) {
        if (!index.IsInRange(0, capacity))
            return;

        selectableArray.Select(index);
    }

    public virtual void SelectAll(bool active) {
        int currentSelectCount = cursor.Length;
        for (int i = 0; i < currentSelectCount; i++)
            Select(cursor.Last());

        if (!active)
            return;
        
        for (int i = 0; i < capacity; i++)
            Select(i);
    }

    public virtual void Remove(int index) {
        if (!index.IsInRange(0, capacity))
            return;
        
        int refreshPage = GetRefreshPageAfterRemoved();
        storage.Remove(selections[index]);
        SetPage(refreshPage);
    }

    public virtual int GetRefreshPageAfterRemoved() {
        bool isIsolated = resultStorage.Count % capacity == 1;
        int refreshPage = Mathf.Max(0, isIsolated ? (page - 1) : page);
        return refreshPage;
    }

    public virtual void Replace(T newItem, int index) {
        if (!index.IsInRange(0, capacity))
            return;

        T oldItem = selections[index];
        storage.Update(oldItem, newItem);
    }

    public virtual void Swap(T newItem, int index) {
        if (!index.IsInRange(0, capacity))
            return;

        T oldItem = selections[index];
        storage.Swap(storage.IndexOf(oldItem), storage.IndexOf(newItem));
    }

    public virtual void ResetFilter(int defaultSelectPage = 0) {
        filter.SetFilterOptions();
        SetPage(defaultSelectPage);
    }

    public virtual void Filter(Func<T, bool> predicate, int defaultSelectPage = 0) {
        filter.SetFilterOptions(predicate);
        SetPage(defaultSelectPage);
    }

    public virtual void Filter(Func<T, int, bool> predicate, int defaultSelectPage = 0) {
        filter.SetFilterOptions(predicate);
        SetPage(defaultSelectPage);
    }

    public virtual void Sort<TKey>(Func<T, TKey> predicate, bool desc = true, int defaultSelectPage = 0) {
        if (predicate == null) {
            filter.SetSortingOptions();
        } else {
            filter.SetSortingOptions(desc, (x) => predicate.Invoke(x));
        }
        SetPage(defaultSelectPage);
    }
}

public enum PageMode {
    NewPage = 0,
    Indent = 1,
}