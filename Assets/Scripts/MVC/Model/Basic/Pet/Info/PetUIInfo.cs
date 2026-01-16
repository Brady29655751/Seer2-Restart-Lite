using System;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PetUIInfo
{
    public const int DATA_COL = 4;
    public int id, subId, baseId, star;
    public int generation => int.Parse(options.Get("generation", "1"));

    public int defaultId, defaultAnimId;

    public int defaultSkinId;
    public List<int> specialSkinList = new List<int>();
    
    public int defaultFeatureId => defaultFeatureList.FirstOrDefault();
    public List<int> defaultFeatureList = new List<int>();

    public List<int> defaultBuffIds = new List<int>();
    public List<Buff> defaultBuffs => defaultBuffIds.Select(x => new Buff(x)).ToList();


    public bool hide = false;
    public Dictionary<string, string> options = new Dictionary<string, string>();

    public Sprite icon => PetUISystem.GetPetIcon(defaultSkinId);
    public Sprite emblemIcon => PetUISystem.GetEmblemIcon(baseId);
    public Sprite battleImage => PetUISystem.GetPetBattleImage(defaultSkinId);
    public Sprite idleImage => PetUISystem.GetPetIdleImage(defaultSkinId);

    public GameObject GetBattleAnim(PetAnimationType type)
    {
        return PetUISystem.GetPetAnimInstance(defaultSkinId, type);
    }

    public void GetBattleAnimAsync(PetAnimationType type, Action<GameObject> onSuccess, Action<float> onProgress = null)
    {
        PetUISystem.GetPetAnimInstanceAsync(defaultSkinId, type, onSuccess, onProgress);
    }

    public void PreloadPetAnimAsync(Action onSuccess = null, Action<float> onProgress = null) 
    {
        PetUISystem.PreloadPetAnimAsync(defaultSkinId, onSuccess, onProgress);
    }

    public PetUIInfo(int petId, int petBaseId) {
        id = petId;
        subId = 0;
        baseId = petBaseId;
        defaultId = petId;
        defaultSkinId = petId;
        defaultAnimId = petId;
        defaultFeatureList = new List<int>(){ petBaseId };
    }

    public PetUIInfo(string[] _data, int startIndex = 0) {
        string[] _slicedData = new string[DATA_COL];
        Array.Copy(_data, startIndex, _slicedData, 0, _slicedData.Length);

        id = int.Parse(_slicedData[0]);
        baseId = int.Parse(_slicedData[1]);
        specialSkinList = _slicedData[2].ToIntList('/');
        options.ParseOptions(_slicedData[3]);

        ParseDefaultId(options.Get("default_id", id.ToString()));
        defaultSkinId = int.Parse(options.Get("default_skin", id.ToString()));
        defaultAnimId = int.Parse(options.Get("default_anim", defaultSkinId.ToString()));
        defaultFeatureList = options.Get("default_feature", baseId.ToString()).ToIntList('/');
        defaultBuffIds = options.Get("default_buff", "none").ToIntList('/');

        hide = bool.Parse(options.Get("hide", "false"));
        star = int.Parse(options.Get("star", "0"));
    }

    private void ParseDefaultId(string idStr) {
        var split = idStr.Split('.');
        defaultId = int.Parse(split[0]);
        if (split.Length == 1) {    
            subId = 0;
            return;
        }

        subId = int.Parse(split[1]);
    }

    public string[] GetRawInfoStringArray() {
        // If no info, then no need to write.
        if (ListHelper.IsNullOrEmpty(specialSkinList) && ListHelper.IsNullOrEmpty(options))
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

    public List<int> GetAllSkinList(PetUI currentPetUI) {
        var currentSkinId = currentPetUI.skinId;
        var allEvovlePetIds = PetExpSystem.GetEvolveChain(baseId, id);
        var allDefaultSkinIds = allEvovlePetIds.Select(x => Pet.GetPetInfo(x).ui.defaultSkinId).Distinct().ToList();
        if (!allDefaultSkinIds.Contains(defaultSkinId))
            allDefaultSkinIds.Add(defaultSkinId);

        var allSkinList = specialSkinList.Concat(allDefaultSkinIds).ToList();
        if (!ListHelper.IsNullOrEmpty(currentPetUI.specialSkinList))
            allSkinList.AddRange(currentPetUI.specialSkinList);

        if (currentSkinId != 0) {
            allSkinList.Remove(currentSkinId);
            allSkinList.Insert(0, currentSkinId);
        }
        return allSkinList;
    }
}
