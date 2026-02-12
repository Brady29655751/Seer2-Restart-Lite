using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CookController : UIModule
{
    
    [SerializeField] private PetItemController cookItemController;
    [SerializeField] private GameObject cookDetailObject;
    [SerializeField] private SelectNumController selectNumController;
    [SerializeField] private PetItemController cookMaterialController;

    [SerializeField] private PetItemBlockView cookItemBlockView;
    [SerializeField] private ItemDetailView cookItemDetailView;

    public int currentCookNum => selectNumController.GetInputValue();
    public Item currentCookItem => cookItemController.GetSelectedItem()[0];
    public FusionRecipe currentCookRecipe => GetCurrentCookRecipe();
    public List<Item> currentCookMaterials => currentCookRecipe?.items ?? new List<Item>();

    protected override void Awake() {
        selectNumController.onValueChangedEvent += OnCurrentCookNumChanged;
    }

    private void OnCurrentCookNumChanged(int num) {
        cookItemDetailView.SetOtherInfo(currentCookMaterials.Select(x => x.name + " x " + (x.num * currentCookNum)).ConcatToString("\n"));
    }

    public FusionRecipe GetCurrentCookRecipe() {
        var recipeKeys = new Dictionary<string, string>()
        {
            {"item", "recipe"},
        };

        var effect = new Effect(currentCookItem.effects[0]);
        effect.abilityOptionDict.Set("result", effect.abilityOptionDict.Get("result") ?? currentCookItem.info.getId.ToString());

        return FusionRecipe.Parse(effect, recipeKeys);
    }

    public void SetMode(bool isSelectMode) {
        cookItemController.gameObject.SetActive(isSelectMode);
        cookDetailObject.SetActive(!isSelectMode);
    }    
    
    public void SetCookItemStorage(List<Item> storage) {
        cookItemController.SetItemBag(storage);
    }
    
    public void OnSelectCookItem(int index) {
        cookItemController.Select(index);
        
        SetMode(false);
        SetInfoPromptActive(false);
        
        OnSetCurrentCookItem();
    }

    public void Cook() {
        if (currentCookMaterials.Exists(x => (Item.Find(x.id)?.num ?? 0) < (x.num * currentCookNum))) {
            Hintbox.OpenHintboxWithContent("材料不足无法制作", 16);
            return;
        }

        currentCookMaterials.ForEach(x => Item.Remove(x.id, x.num * currentCookNum));
        var results = currentCookRecipe.resultIds.Random(currentCookNum, true, currentCookRecipe.resultWeights)
            .GroupBy(x => x).Select(x => new Item(x.Key, x.Count()));

        foreach (var item in results)
        {
            Item.Add(item);
            Item.OpenHintbox(item);
        }   

        OnSetCurrentCookItem();
    }

    public void OnSetCurrentCookItem() {
        cookItemBlockView.SetItem(currentCookItem);
        cookItemDetailView.SetItem(currentCookItem);
        cookMaterialController.SetItemBag(currentCookMaterials.Select(x => new Item(x.id, Item.Find(x.id)?.num ?? 0)).ToList());

        selectNumController.SetMaxValue(99);
        OnCurrentCookNumChanged(currentCookNum);
    }
}
