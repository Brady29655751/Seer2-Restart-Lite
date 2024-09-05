using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PetSelectController : Module
{
    [SerializeField] private PetSelectModel selectModel;
    [SerializeField] private PetSelectView selectView;
    [SerializeField] private PageView pageView;

    public event Action onSetStorageEvent;
    public event Action<Pet> onSelectPetEvent;
    public event Action<int> onSelectIndexEvent;

    protected override void Awake()
    {
        base.Awake();
    }

    public override void Init()
    {
        base.Init();
    }

    public int[] GetCursor() {
        return selectModel.cursor;
    }

    public Pet[] GetPetSelections() {
        return selectModel.selections;
    }

    public void RefreshView() {
        selectView.SetSelections(selectModel.selections);
        selectView.SetStorageCountText(selectModel.count);
        if (selectModel.cursor.Length > 0) {
            selectView.Select(selectModel.cursor[0]);
        }
        pageView?.SetPage(selectModel.page, selectModel.lastPage);
    }

    public void SetStorage(List<Pet> storage, int defaultSelectPage = 0) {
        selectModel.SetStorage(storage, defaultSelectPage);
        onSetStorageEvent?.Invoke();
    }

    public void Select(int index) {
        selectModel.Select(index);
        RefreshView();
        if (!index.IsInRange(0, selectModel.selections.Length))
            return;
        
        onSelectPetEvent?.Invoke(selectModel.selections[index]);
        onSelectIndexEvent?.Invoke(index);
    }

    public void PrevPage(bool defaultSelect = true) {
        selectModel.PrevPage();
        if ((defaultSelect) && (selectModel.cursor.Length <= 0)) {
            Select(0);
        }
    }

    public void NextPage(bool defaultSelect = true) {
        selectModel.NextPage();
        if ((defaultSelect) && (selectModel.cursor.Length <= 0)) {
            Select(0);
        }
    }

    public void SetPageByInputField(bool defaultSelect = true) {
        if (pageView == null)
            return;

        int page = pageView.GetJumpPage();
        if (page < 0)
            return;

        selectModel.SetPage(page);
        if ((defaultSelect) && (selectModel.cursor.Length <= 0)) {
            Select(0);
        }
    }

    public void Sort(Func<Pet, object> sorter) {
        selectModel.Sort(sorter);
        Select(0);
    }

    public void Filter(Func<Pet, bool> filter) {
        selectModel.Filter(filter);
        Select(0);
    }

    public void Filter(Func<Pet, int, bool> filter) {
        selectModel.Filter(filter);
        Select(0);
    }


}
