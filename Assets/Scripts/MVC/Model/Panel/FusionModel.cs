using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class FusionModel : Module
{
    public List<FusionRecipe> currentRecipeList = new List<FusionRecipe>();
    public int databaseRecipePage = 0, currentRecipePage = 0;
    public FusionRecipe currentRecipe => currentRecipeList?.ElementAtOrDefault(currentRecipePage) ?? new FusionRecipe();

    private int mainPetIndex, subPetIndex;
    public Pet mainPet, subPet;

    private bool TrySetRecipe()
    {
        databaseRecipePage = currentRecipePage = 0;

        if (mainPet == null || subPet == null)
            return false;

        currentRecipeList = PetFusionSystem.GetFusionRecipeList(mainPet, subPet);
        return !currentRecipe.IsEmpty();
    }


    public void SetMainPet(Pet pet)
    {
        mainPet = pet;
        mainPetIndex = (int)Player.GetSceneData("PetSelectIndex", 0);
        TrySetRecipe();
    }

    public void SetSubPet(Pet pet)
    {
        subPet = pet;
        subPetIndex = (int)Player.GetSceneData("PetSelectIndex", 0);
        TrySetRecipe();
    }

    public bool TryFusion(out int resultPetId)
    {
        resultPetId = 0;

        if (mainPet == null || subPet == null)
        {
            Hintbox.OpenHintboxWithContent("请选择主副精灵", 16);
            return false;
        }

        if (mainPetIndex == subPetIndex)
        {
            Hintbox.OpenHintboxWithContent("不能选择相同的精灵", 16);
            return false;
        }

        if (currentRecipe.IsEmpty())
        {
            Hintbox.OpenHintboxWithContent("这两个精灵的元神不相容哦！", 16);
            return false;
        }

        if ((currentRecipe.baseId.Item1 != currentRecipe.baseId.Item2) && (currentRecipe.baseId.Item1 == subPet.basic.baseId) && (currentRecipe.baseId.Item2 == mainPet.basic.baseId))
        {
            Hintbox.OpenHintboxWithContent("主副精灵放反了哦！", 16);
            return false;
        }

        var mainChainCount = PetExpSystem.GetEvolveChain(currentRecipe.petId.Item1, mainPet.id)?.Count ?? 0;
        if (mainChainCount == 0)
        {
            Hintbox.OpenHintboxWithContent("主精灵未达到可融合形态哦！", 16);
            return false;
        }

        var subChainCount = PetExpSystem.GetEvolveChain(currentRecipe.petId.Item2, subPet.id)?.Count ?? 0;
        if (subChainCount == 0)
        {
            Hintbox.OpenHintboxWithContent("副精灵未达到可融合形态哦！", 16);
            return false;
        }

        var lackItem = currentRecipe.items.FirstOrDefault(x => (x != null) && ((Item.Find(x.id)?.num ?? 0) < x.num));
        if (lackItem != null)
        {
            Hintbox.OpenHintboxWithContent("融合素材<color=#ffbb33>【" + lackItem.name + "】</color>不足！", 14);
            return false;
        }

        resultPetId = currentRecipe.resultPets.Random(currentRecipe.resultWeights);
        return true;
    }

}
