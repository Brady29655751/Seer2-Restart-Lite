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
        var item = fusionModel.fusionItems.ElementAtOrDefault(index);
        if (item == null)
            return;

        infoPrompt.SetItem(item);
    }

    public void PrevPage() {
        fusionModel.recipeIndex--;
        OnSetPage();
    }

    public void NextPage() {
        fusionModel.recipeIndex++;
        OnSetPage();
    }

    private void OnSetPage() {
        int lastPage = fusionModel.fusionRecipeInfos.Count - 1;
        fusionModel.recipeIndex = Mathf.Clamp(fusionModel.recipeIndex, 0, lastPage);
        if (!fusionModel.TryGetRecipe(fusionModel.fusionRecipeInfos[fusionModel.recipeIndex]))
            return;
            
        fusionView.SetMainPet(Pet.GetExamplePet(fusionModel.fusionPetId.Item1));
        fusionView.SetSubPet(Pet.GetExamplePet(fusionModel.fusionPetId.Item2));
        fusionView.SetItem(fusionModel.fusionItems);
        pageView.SetPage(fusionModel.recipeIndex, lastPage);
    }
}
