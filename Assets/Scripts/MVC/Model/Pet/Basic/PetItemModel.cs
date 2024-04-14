using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PetItemModel : SelectModel<Item>
{
    [SerializeField] public List<ItemCategory> categories;
    public Pet currentPet;
    public Item[] items => selections;

    protected override void Awake()
    {
        base.Awake();
        filter.SetFilterOptions(x => categories.Any(y => x.IsInCategory(y)));
    }

    public override void SetStorage(List<Item> storage, int defaultSelectPage = 0)
    {
        this.storage = storage;
        SetPage(defaultSelectPage);
    }

    public void SetPet(Pet pet) {
        currentPet = pet;
    }
}
