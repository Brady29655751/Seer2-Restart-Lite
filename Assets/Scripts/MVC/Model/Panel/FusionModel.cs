using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class FusionModel : Module
{
    private static string[] fusionRecipeKeys => new string[] { "pet", "base", "item", "result" };
    public List<ItemInfo> fusionRecipeInfos => ItemInfo.database.Where(x => (x.type == ItemType.Recipe) && fusionRecipeKeys.All(key => x.effects.Exists(y => y.abilityOptionDict.ContainsKey(key)))).ToList();
    public int recipeIndex = 0;

    private bool isRecipeExist => !ListHelper.IsNullOrEmpty(resultPets);
    private int mainPetIndex, subPetIndex;
    public Pet mainPet, subPet;
    public (int, int) fusionPetId, fusionBaseId;
    public List<Item> fusionItems = new List<Item>(){ null, null, null, null };
    public List<int> resultPets = new List<int>();
    public List<int> resultWeights = new List<int>();

    public bool TryGetRecipe(ItemInfo recipe) {
        var e = recipe.effects;
        for (int j = 0; j < e.Count; j++) {
            if (!TryParseRecipe(e[j], out (int, int) petId, out (int, int) baseId, out List<Item> itemsRequired, out List<(int, int)> resultPetPdf))
                continue;

            fusionPetId = petId;
            fusionBaseId = baseId;
            fusionItems = itemsRequired;
            resultPets = resultPetPdf.Select(x => x.Item1).ToList();
            resultWeights = resultPetPdf.Select(x => x.Item2).ToList();
            return true;         
        }
        return false;
    }

    private bool TrySetRecipe() {
        recipeIndex = 0;
        fusionItems = new List<Item>(){ null, null, null, null };
        resultPets = new List<int>();
        resultWeights = new List<int>();

        if (mainPet == null || subPet == null)
            return false;

        if (fusionRecipeInfos.Count == 0)
            return false;

        for (int i = 0; i < fusionRecipeInfos.Count; i++) {
            var e = fusionRecipeInfos[i].effects;
            for (int j = 0; j < e.Count; j++) {
                if (!TryParseRecipe(e[j], out (int, int) petId, out (int, int) baseId, out List<Item> itemsRequired, out List<(int, int)> resultPetPdf))
                    continue;

                if (((baseId.Item1 == mainPet.basic.baseId) && (baseId.Item2 == subPet.basic.baseId)) || 
                    ((baseId.Item1 == subPet.basic.baseId) && (baseId.Item2 == mainPet.basic.baseId))) 
                {
                    fusionPetId = petId;
                    fusionBaseId = baseId;
                    fusionItems = itemsRequired;
                    resultPets = resultPetPdf.Select(x => x.Item1).ToList();
                    resultWeights = resultPetPdf.Select(x => x.Item2).ToList();
                    return true;
                }
            }
        }

        return false;
    }

    public bool TryParseRecipe(Effect recipe, out (int, int) petId, out (int, int) baseId, out List<Item> itemsRequired, out List<(int, int)> resultPetPdf) {
        petId = (0, 0);
        baseId = (0, 0);
        itemsRequired = new List<Item>();
        resultPetPdf = new List<(int, int)>();

        var options = recipe.abilityOptionDict;
        var petIdList = options.Get("pet").ToIntList('/');
        var baseIdList = options.Get("base").ToIntList('/');
        var itemList = options.Get("item").Split('/');
        var resultPdf = options.Get("result").Split('/');

        petId = (petIdList[0], petIdList[1]);
        baseId = (baseIdList[0], baseIdList[1]);

        for (int i = 0; i < itemList.Length; i++) {
            var index = itemList[i].IndexOf('[');
            var id = (int)Identifier.GetNumIdentifier((index < 0) ? itemList[i] : itemList[i].Substring(0, index));
            var num = (index < 0) ? 1 : (int)Identifier.GetNumIdentifier(itemList[i].TrimParentheses());
            itemsRequired.Add(new Item(id, num));
        }

        for (int i = 0; i < resultPdf.Length; i++) {
            var index = resultPdf[i].IndexOf('[');
            var id = (int)Identifier.GetNumIdentifier((index < 0) ? resultPdf[i]: resultPdf[i].Substring(0, index));
            var probability = (index < 0) ? 1 : (int)Identifier.GetNumIdentifier(resultPdf[i].TrimParentheses());
            resultPetPdf.Add((id, probability));
        }
        return true;
    }


    public void SetMainPet(Pet pet) {
        mainPet = pet;
        mainPetIndex = (int)Player.GetSceneData("PetSelectIndex", 0);
        TrySetRecipe();
    }

    public void SetSubPet(Pet pet) {
        subPet = pet;
        subPetIndex = (int)Player.GetSceneData("PetSelectIndex", 0);
        TrySetRecipe();
    }

    public bool TryFusion(out int resultPetId) {
        resultPetId = 0;

        if (mainPet == null || subPet == null) {
            Hintbox.OpenHintboxWithContent("请选择主副精灵", 16);
            return false;
        }
        if (mainPetIndex == subPetIndex) {
            Hintbox.OpenHintboxWithContent("不能选择相同的精灵", 16);
            return false;
        }

        if (!isRecipeExist) {
            Hintbox.OpenHintboxWithContent("这两个精灵的元神不相容哦！", 16);
            return false;
        }

        if ((fusionBaseId.Item1 != fusionBaseId.Item2) && (fusionBaseId.Item1 == subPet.basic.baseId) && (fusionBaseId.Item2 == mainPet.basic.baseId)) {
            Hintbox.OpenHintboxWithContent("主副精灵放反了哦！", 16);
            return false;
        }

        var mainChainCount = PetExpSystem.GetEvolveChain(fusionPetId.Item1, mainPet.id)?.Count ?? 0;
        if (mainChainCount == 0) {
            Hintbox.OpenHintboxWithContent("主精灵未达到可融合形态哦！", 16);
            return false;
        }

        var subChainCount = PetExpSystem.GetEvolveChain(fusionPetId.Item2, subPet.id)?.Count ?? 0;
        if (subChainCount == 0) {
            Hintbox.OpenHintboxWithContent("副精灵未达到可融合形态哦！", 16);
            return false;
        }

        var lackItem = fusionItems.FirstOrDefault(x => (x != null) && ((Item.Find(x.id)?.num ?? 0) < x.num));
        if (lackItem != null) {
            Hintbox.OpenHintboxWithContent("融合素材<color=#ffbb33>【" + lackItem.name + "】</color>不足！", 14);
            return false;
        }
        
        resultPetId = resultPets.Random(resultWeights);
        return true;
    }
    
}
