using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorkshopLearnBuffModel : SelectModel<BuffInfo>
{
    [SerializeField] private IInputField searchInputField, idInputField;

    public int id => (int.TryParse(idInputField.inputString, out var buffId)) ? buffId : 0;
    public BuffInfo currentBuffInfo => Buff.GetBuffInfo(id);


    protected override void Awake()
    {
        base.Awake();
        SetStorage(BuffInfo.database);
        Sort(x => x.id);
    }


    public void Search() {
        if (string.IsNullOrEmpty(searchInputField.inputString)) {
            Hintbox.OpenHintboxWithContent("搜索名称不能为空！", 16);
            return;
        }

        idInputField.SetInputString(string.Empty);

        // Try search buffs with same name.
        if (int.TryParse(searchInputField.inputString, out var buffId))
            Filter(x => x.id == buffId);
        else
            Filter(x => x.name == searchInputField.inputString);

        if (count > 0)
            return;

        // No buffs with same name. Search similar result.
        Filter(x => x.name.Contains(searchInputField.inputString));
    }

    public override void Select(int index) {
        if (!index.IsInRange(0, selectionSize)) {
            idInputField.SetInputString(string.Empty);
            return;
        }

        idInputField.SetInputString(selections[index]?.id.ToString() ?? string.Empty);
    }

    public bool VerifyDIYLearnBuff(out string error) {
        error = string.Empty;

        if (string.IsNullOrEmpty(idInputField.inputString)) {
            error = "印记序号不能为空！";
            return false;
        }

        if (Buff.GetBuffInfo(id) == null) {
            error = "该印记序号不存在！";
            return false;
        }

        return true;
    }
}
