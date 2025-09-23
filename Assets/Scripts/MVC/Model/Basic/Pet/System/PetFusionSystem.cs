using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PetFusionSystem
{
    public static string[] fusionRecipeKeys => new string[] { "pet", "base", "item", "result" };
    public static List<FusionRecipe> fusionRecipes => ItemInfo.database.Where(x => x.type == ItemType.Recipe).Select(x => x.effects.Select(FusionRecipe.Parse)).SelectMany(x => x).Where(x => x != null).ToList();

    public static List<FusionRecipe> GetFusionRecipeList(Pet mainPet, Pet subPet)
    {
        if (mainPet == null || subPet == null)
            return null;

        if (ListHelper.IsNullOrEmpty(PetFusionSystem.fusionRecipes))
            return null;

        return fusionRecipes.Where(recipe =>
            ((recipe.baseId.Item1 == mainPet.basic.baseId) && (recipe.baseId.Item2 == subPet.basic.baseId)) ||
            ((recipe.baseId.Item1 == subPet.basic.baseId) && (recipe.baseId.Item2 == mainPet.basic.baseId))
        ).ToList();
    }
}

public class FusionRecipe
{

    public (int, int) petId = (0, 0);
    public (int, int) baseId = (0, 0);
    public List<Item> items = new List<Item>() { null, null, null, null };
    public List<int> resultPets = new List<int>();
    public List<int> resultWeights = new List<int>();

    public static FusionRecipe Parse(Effect effect)
    {
        var options = effect.abilityOptionDict;
        if (!PetFusionSystem.fusionRecipeKeys.All(options.ContainsKey))
            return null;

        var petIdList = options.Get("pet").ToIntList('/');
        var baseIdList = options.Get("base").ToIntList('/');
        var itemList = options.Get("item").Split('/');
        var resultPdf = options.Get("result").Split('/');
        var recipe = new FusionRecipe();

        recipe.petId = (petIdList[0], petIdList[1]);
        recipe.baseId = (baseIdList[0], baseIdList[1]);
        recipe.items = itemList.Select(itemExpr =>
        {
            var index = itemExpr.IndexOf('[');
            var id = (int)Identifier.GetNumIdentifier((index < 0) ? itemExpr : itemExpr.Substring(0, index));
            var num = (index < 0) ? 1 : (int)Identifier.GetNumIdentifier(itemExpr.TrimParentheses());

            return new Item(id, num);
        }).ToList();

        for (int i = 0; i < resultPdf.Length; i++)
        {
            var index = resultPdf[i].IndexOf('[');
            var id = (int)Identifier.GetNumIdentifier((index < 0) ? resultPdf[i] : resultPdf[i].Substring(0, index));
            var probability = (index < 0) ? 1 : (int)Identifier.GetNumIdentifier(resultPdf[i].TrimParentheses());

            recipe.resultPets.Add(id);
            recipe.resultWeights.Add(probability);
        }

        return recipe;
    }

    public bool IsEmpty() => ListHelper.IsNullOrEmpty(resultPets);
}
