using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PetFusionSystem
{
    public static string[] fusionRecipeKeys => new string[] { "pet", "base", "item", "result" };
    public static List<FusionRecipe> fusionRecipes => ItemInfo.database.Where(x => x.type == ItemType.Recipe)
        .Select(x => x.effects.Select(effect => FusionRecipe.Parse(effect, mustIncludeKeys: fusionRecipeKeys)))
        .SelectMany(x => x).Where(x => x != null).ToList();

    public static List<FusionRecipe> GetFusionRecipeList(Pet mainPet, Pet subPet)
    {
        if (mainPet == null || subPet == null)
            return null;

        if (ListHelper.IsNullOrEmpty(PetFusionSystem.fusionRecipes))
            return null;

        return fusionRecipes.Where(recipe =>
            ((recipe.baseId.Item1 == mainPet.basic.baseId) && (recipe.baseId.Item2 == subPet.basic.baseId))
            // || ((recipe.baseId.Item1 == subPet.basic.baseId) && (recipe.baseId.Item2 == mainPet.basic.baseId))
        ).ToList();
    }
}

public class FusionRecipe
{

    public (int, int) petId = (0, 0);
    public (int, int) baseId = (0, 0);
    public (int?, int?) gender = (null, null);
    public List<Item> items = new List<Item>() { null, null, null, null };
    public List<int> resultIds = new List<int>();
    public List<int> resultWeights = new List<int>();

    public static FusionRecipe Parse(Effect effect, IDictionary<string, string> recipeKeys = null, IEnumerable<string> mustIncludeKeys = null)
    {
        if (effect == null)
            return null;

        var mergedKeys = new Dictionary<string, string>().Merge(recipeKeys);
        var options = effect.abilityOptionDict;

        if (!ListHelper.IsNullOrEmpty(mustIncludeKeys) && !mustIncludeKeys.All(options.ContainsKey))
            return null;

        var petIdList = options.Get(mergedKeys.Get("pet", "pet"))?.ToIntList('/');
        var baseIdList = options.Get(mergedKeys.Get("base", "base"))?.ToIntList('/');
        var genderList = options.Get(mergedKeys.Get("gender", "gender"))?.ToIntList('/');
        var itemList = options.Get(mergedKeys.Get("item", "item"))?.Split('/');
        var resultPdf = options.Get(mergedKeys.Get("result", "result"))?.Split('/');
        
        var recipe = new FusionRecipe();

        if (!ListHelper.IsNullOrEmpty(petIdList))
            recipe.petId = (petIdList[0], petIdList[1]);
        
        if (!ListHelper.IsNullOrEmpty(baseIdList))
            recipe.baseId = (baseIdList[0], baseIdList[1]);

        if (!ListHelper.IsNullOrEmpty(genderList))
            recipe.gender = (genderList[0], genderList[1]);
        
        if (!ListHelper.IsNullOrEmpty(itemList))
        {
            recipe.items = itemList.Select(itemExpr =>
            {
                var index = itemExpr.IndexOf('[');
                var id = (int)Identifier.GetNumIdentifier((index < 0) ? itemExpr : itemExpr.Substring(0, index));
                var num = (index < 0) ? 1 : (int)Identifier.GetNumIdentifier(itemExpr.TrimParentheses());

                return new Item(id, num);
            }).ToList();   
        }

        if (!ListHelper.IsNullOrEmpty(resultPdf))
        {
            for (int i = 0; i < resultPdf.Length; i++)
            {
                var index = resultPdf[i].IndexOf('[');
                var id = (int)Identifier.GetNumIdentifier((index < 0) ? resultPdf[i] : resultPdf[i].Substring(0, index));
                var probability = (index < 0) ? 1 : (int)Identifier.GetNumIdentifier(resultPdf[i].TrimParentheses());

                recipe.resultIds.Add(id);
                recipe.resultWeights.Add(probability);
            }   
        }

        return recipe;
    }

    public bool IsEmpty() => ListHelper.IsNullOrEmpty(resultIds);
    public bool IsPetEmpty() => petId.Item1 == 0 || petId.Item2 == 0;
}
