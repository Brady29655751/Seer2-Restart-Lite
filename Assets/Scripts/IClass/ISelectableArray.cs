using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ISelectableArray<T>
{
    // number of pets player should select
    public int selectCount { get; protected set; } = 1;
    public int capacity { get; protected set; } = 6;
    public int size { get => items.Count(x => x != null); }
    public int length { get => items.Length; }
    public bool[] isSelected { get; protected set; }
    public int[] currentSelectIndex { get => isSelected?.AllIndexOf(true); }
    public T[] currentSelectItems { get => items.Where((x, i) => isSelected[i]).ToArray(); }
    
    // number of items player currently selects
    public int currentSelectCount { get => isSelected.Count(x => x); }
    public T[] items { get; protected set; }
    public T this[int n] {
        get => (n.IsInRange(0, items.Length)) ? items[n] : default(T);
    }

    public bool[] locks { get; protected set; }

    private Action<int> OnSelectCallback;

    public ISelectableArray() { 
        SetSelectCount(1);
        SetCapacity(6); 
    }

    public ISelectableArray(int capacity) {
        SetSelectCount(1);
        SetCapacity(capacity);
    }

    public void SetArray(T[] array, bool keepSelected = false) {
        items = array;
        SetCapacity(Mathf.Max(array.Length, capacity));
        InitLock();
        InitSelectedFlag(keepSelected);
    }

    public void InitSelectedFlag(bool keepSelected) {
        int[] oldSelected = currentSelectIndex;
        isSelected = new bool[capacity];
        if (keepSelected && (oldSelected != null)) {
            foreach (var index in oldSelected)
                Select(index);
        }
    }

    private void InitLock() {
        locks = new bool[capacity];
    }

    public void Lock(int[] _locks) {
        if (locks == null)
            return;
        foreach (var id in _locks) {
            if (id <= locks.Length)
                continue;
            locks[id] = true;
        }
    }

    public void UnLock(int[] _keys) {
        if (locks == null)
            return;
        foreach (var id in _keys) {
            if (id <= locks.Length)
                continue;
            locks[id] = false;
        }
    }

    public void SetSelectCount(int count) {
        int diff = selectCount - count;
        if ((diff == 0) || (count <= 0))
            return;

        if (diff > 0) {
            int s = currentSelectCount;
            for(int i = 0; i < s; i++) {
                Select(currentSelectIndex.Last());
            }
        }
        selectCount = count;
    }

    public void SetCapacity(int c) {
        if (c <= 0)
            return;
        capacity = c;
    }

    public void Select(int index) {
        if ((items == null) || (items.Length <= index))
            return;
        if (locks[index])
            return;
        if (selectCount == 1) {
            if (isSelected[index])
                return;
            if (currentSelectCount > 0)
                isSelected[currentSelectIndex[0]] = false;
            isSelected[index] = true;
            OnSelectCallback?.Invoke(index);
            return;
        }
        isSelected[index] = !isSelected[index];
        OnSelectCallback?.Invoke(index);
    }

    public void SetOnSelectCallback(Action<int> callback) {
        OnSelectCallback = callback;
    }
    
}
