using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FusionRecipePanel : Panel
{
    [SerializeField] private FusionModel fusionModel;
    [SerializeField] private FusionView fusionView;
    [SerializeField] private PageView pageView;

    public override void Init()
    {
        base.Init();
        OnSetPage();
    }

    public void ShowItemInfo(int index) {
        var item = fusionModel.currentRecipe.items.ElementAtOrDefault(index);
        if (item == null)
            return;

        infoPrompt.SetItem(item);
    }

    public void PrevPage() {
        fusionModel.databaseRecipePage--;
        OnSetPage();
    }

    public void NextPage() {
        fusionModel.databaseRecipePage++;
        OnSetPage();
    }

    private void OnSetPage() {
        int lastPage = Mathf.Max(PetFusionSystem.fusionRecipes.Count - 1, 0);
        fusionModel.databaseRecipePage = Mathf.Clamp(fusionModel.databaseRecipePage, 0, lastPage);
        pageView.SetPage(fusionModel.databaseRecipePage, lastPage);

        var recipe = PetFusionSystem.fusionRecipes.Get(fusionModel.databaseRecipePage, null);
        if (recipe == null)
            return;

        fusionView.SetMainPet(Pet.GetExamplePet(recipe.petId.Item1));
        fusionView.SetSubPet(Pet.GetExamplePet(recipe.petId.Item2));
        fusionView.SetItem(recipe.items);
    }
}
