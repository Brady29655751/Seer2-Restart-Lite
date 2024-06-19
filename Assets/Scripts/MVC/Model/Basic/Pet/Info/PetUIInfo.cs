using System;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PetUIInfo
{
    public const int DATA_COL = 4;
    public int id, baseId;

    public int defaultSkinId;
    public List<int> specialSkinList = new List<int>();
    
    public int defaultFeatureId => defaultFeatureList.FirstOrDefault();
    public List<int> defaultFeatureList = new List<int>();

    public List<int> defaultBuffIds = new List<int>();
    public List<Buff> defaultBuffs => defaultBuffIds.Select(x => new Buff(x)).ToList();

    public Dictionary<string, string> options = new Dictionary<string, string>();

    public Sprite icon => PetUISystem.GetPetIcon(defaultSkinId);
    public Sprite emblemIcon => PetUISystem.GetEmblemIcon(baseId);
    public Sprite battleImage => PetUISystem.GetPetBattleImage(defaultSkinId);

    public PetUIInfo(int petId, int petBaseId) {
        id = petId;
        baseId = petBaseId;
        defaultSkinId = petId;
        defaultFeatureList = new List<int>(){ petBaseId };
    }

    public PetUIInfo(string[] _data, int startIndex = 0) {
        string[] _slicedData = new string[DATA_COL];
        Array.Copy(_data, startIndex, _slicedData, 0, _slicedData.Length);

        id = int.Parse(_slicedData[0]);
        baseId = int.Parse(_slicedData[1]);
        specialSkinList = _slicedData[2].ToIntList('/');
        options.ParseOptions(_slicedData[3]);

        defaultSkinId = int.Parse(options.Get("default_skin", id.ToString()));
        defaultFeatureList = options.Get("default_feature", baseId.ToString()).ToIntList('/');
        defaultBuffIds = options.Get("default_buff", "none").ToIntList('/');
    }

    public string[] GetRawInfoStringArray() {
        // If no info, then no need to write.
        if (List.IsNullOrEmpty(specialSkinList) && List.IsNullOrEmpty(options))
            return null;

        var rawSkinList = specialSkinList.Select(x => x.ToString()).ConcatToString("/");
        
        return new string[] { id.ToString(), baseId.ToString(), ((specialSkinList.Count == 0) ? "none" : rawSkinList),
            GetRawOptionString() };
    }

    public string GetRawOptionString() {
        if (options.Count == 0)
            return "none";

        return options.Select(entry => entry.Key + "=" + entry.Value).ConcatToString("&");
    }

    public List<int> GetAllSkinList(int currentSkinId) {
        var allEvovlePetIds = Pet.GetPetInfo(id).allEvolvePetIds;
        var allDefaultSkinIds = allEvovlePetIds.Select(x => Pet.GetPetInfo(x).ui.defaultSkinId).Distinct().ToList();
        if (!allDefaultSkinIds.Contains(defaultSkinId))
            allDefaultSkinIds.Add(defaultSkinId);

        var allSkinList = specialSkinList.Concat(allDefaultSkinIds).ToList();
        if (currentSkinId != 0) {
            allSkinList.Remove(currentSkinId);
            allSkinList.Insert(0, currentSkinId);
        }
        return allSkinList;
    }
}
