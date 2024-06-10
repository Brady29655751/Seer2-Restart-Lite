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
    public List<Item> currentCookRecipe => GetCurrentCookRecipe();

    protected override void Awake() {
        selectNumController.onValueChangedEvent += OnCurrentCookNumChanged;
    }

    private void OnCurrentCookNumChanged(int num) {
        cookItemDetailView.SetOtherInfo(currentCookRecipe.Select(x => x.name + " x " + (x.num * currentCookNum)).ConcatToString("\n"));
    }

    public List<Item> GetCurrentCookRecipe() {
        var recipe = currentCookItem.effects[0]?.abilityOptionDict.Get("recipe", string.Empty).Split('/');
        var materials = new List<Item>();

        for (int i = 0; i < recipe.Length; i++) {
            var id = int.Parse(recipe[i].Substring(0, recipe[i].IndexOf('[')));
            var num = int.Parse(recipe[i].TrimParentheses());
            materials.Add(new Item(id, num));
        }

        return materials;
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
        var item = new Item(currentCookItem.id, currentCookNum);
        if (currentCookRecipe.Exists(x => (Item.Find(x.id)?.num ?? 0) < (x.num * item.num))) {
            Hintbox.OpenHintboxWithContent("材料不足无法制作", 16);
            return;
        }

        currentCookRecipe.ForEach(x => Item.Remove(x.id, x.num * currentCookNum));
        Item.Add(item);
        Item.OpenHintbox(item);

        OnSetCurrentCookItem();
    }

    public void OnSetCurrentCookItem() {
        cookItemBlockView.SetItem(currentCookItem);
        cookItemDetailView.SetItem(currentCookItem);
        cookMaterialController.SetItemBag(currentCookRecipe.Select(x => new Item(x.id, Item.Find(x.id)?.num ?? 0)).ToList());

        selectNumController.SetMaxValue(99);
        OnCurrentCookNumChanged(currentCookNum);
    }
}
