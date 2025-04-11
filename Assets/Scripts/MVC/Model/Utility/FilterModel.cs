using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FilterModel<T> : Module
{
    [SerializeField] private IInputField inputField;
    public bool isWorking => !string.IsNullOrEmpty(inputString?.Trim());
    public string inputString => inputField.inputString;
    public Func<T, bool> filter => GetFilter();
    public FilterType type => GetFilterType();

    public virtual FilterType GetFilterType() {
        if (IsIDFilter())
            return FilterType.ID;

        if (IsParanthesesFilter())
            return FilterType.Parentheses;

        return FilterType.Name;
    }

    public virtual bool IsIDFilter() {
        return int.TryParse(inputString, out _);
    }

    public virtual bool IsParanthesesFilter() {
        return inputString?.TryTrimParentheses(out _, '(', ')') ?? false;
    }

    public virtual Func<T, bool> GetFilter() {
        return (x) => true;
    }
}

public enum FilterType {
    Name = 0,
    ID = 1,
    Parentheses = 2,
}
