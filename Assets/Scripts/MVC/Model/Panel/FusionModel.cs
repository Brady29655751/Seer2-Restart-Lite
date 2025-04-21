using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class FusionModel : Module
{
    private bool isRecipeExist => !ListHelper.IsNullOrEmpty(resultPets);
    private int mainPetIndex, subPetIndex;
    public Pet mainPet, subPet;
    public List<Item> fusionItems = new List<Item>(){ null, null, null, null };
    public List<int> resultPets = new List<int>();
    public List<int> resultWeights = new List<int>();

    private bool TrySetRecipe() {
        fusionItems = new List<Item>(){ null, null, null, null };
        resultPets = new List<int>();
        resultWeights = new List<int>();

        if (mainPet == null || subPet == null)
            return false;

        var recipeInfos = ItemInfo.database.Where(x => x.type == ItemType.Recipe).ToList();
        if (recipeInfos.Count == 0)
            return false;

        for (int i = 0; i < recipeInfos.Count; i++) {
            var e = recipeInfos[i].effects;
            for (int j = 0; j < e.Count; j++) {
                if (!TryParseRecipe(e[j], out int mainPetId, out int subPetId, out List<Item> itemsRequired, out List<(int, int)> resultPetPdf))
                    continue;

                if ((mainPetId != mainPet.id) || (subPetId != subPet.id))
                    continue;

                fusionItems = itemsRequired;
                resultPets = resultPetPdf.Select(x => x.Item1).ToList();
                resultWeights = resultPetPdf.Select(x => x.Item2).ToList();
                return true;
            }
        }

        return false;
    }

    private bool TryParseRecipe(Effect recipe, out int mainPetId, out int subPetId, out List<Item> itemsRequired, out List<(int, int)> resultPetPdf) {
        mainPetId = subPetId = 0;
        itemsRequired = new List<Item>();
        resultPetPdf = new List<(int, int)>();

        var options = recipe.abilityOptionDict;
        var pet = options.Get("pet");
        var item = options.Get("item");
        var result = options.Get("result");
        if (pet == null || item == null || result == null)
            return false;

        var petId = pet.ToIntList('/');
        var itemList = item.Split('/');
        var resultPdf = result.Split('/');

        mainPetId = petId[0];
        subPetId = petId[1];

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

        var lackItem = fusionItems.FirstOrDefault(x => (x != null) && ((Item.Find(x.id)?.num ?? 0) < x.num));
        if (lackItem != null) {
            Hintbox.OpenHintboxWithContent("融合素材<color=#ffbb33>【" + lackItem.name + "】</color>不足！", 14);
            return false;
        }
        
        resultPetId = resultPets.Random(resultWeights);
        return true;
    }
    
}
