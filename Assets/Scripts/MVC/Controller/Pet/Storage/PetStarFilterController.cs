using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PetStarFilterController : Module
{
    [SerializeField] List<IButton> starButtons;
    public event Action<Func<Pet, bool>> onFilterEvent;
    private int currentStar = 0;

    public override void Init()
    {
        base.Init();
        SetActive(false);
    }

    public Func<Pet, bool> GetFilter(int star)
    {
        return pet => (pet != null) && ((star <= 0) || (pet.info.star == star));
    }

    public void SetActive(bool active)
    {
        if (active)
            return;

        SetStar(0);
    }

    public void SetStar(int star)
    {
        if (star == currentStar)
            currentStar = 0;
        else
            currentStar = star;

        for (int i = 0; i < starButtons.Count; i++)
        {
            var color = (i < currentStar) ? Color.white : Color.gray;
            starButtons[i]?.image?.SetColor(color);
        }
    }

    public void Filter()
    {
        onFilterEvent?.Invoke(GetFilter(currentStar));
    }

    public void Select(int star)
    {
        SetStar(star);
        Filter();
    }
}
