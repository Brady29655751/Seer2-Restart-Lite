using System;
using System.Xml.Serialization;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PetSkillInfo
{
    public const int DATA_COL = 3;

    public int id;
    public List<int> skillIdList;
    public List<Skill> skillList => GetSkillList();

    public List<int> learnLevelList;
    public List<LearnSkillInfo> learnInfoList => GetLearnInfoList();

    public PetSkillInfo() {}
    public PetSkillInfo(string[] _data, int startIndex) {
        string[] _slicedData = new string[DATA_COL];
        Array.Copy(_data, startIndex, _slicedData, 0, _slicedData.Length);

        id = int.Parse(_slicedData[0]);
        skillIdList = _slicedData[1].ToIntList('/');
        learnLevelList = _slicedData[2].ToIntList('/');
    }

    public PetSkillInfo(int id, List<LearnSkillInfo> skillInfoList) {
        this.id = id;
        this.skillIdList = skillInfoList.Select(x => x.skill?.id ?? 0).DefaultIfEmpty(0).ToList();
        this.learnLevelList = skillInfoList.Select(x => ((int)x.secretType) * 10000 + x.value).DefaultIfEmpty(0).ToList();
    }

    public string[] GetRawInfoStringArray() {
        return new string[] { id.ToString(), skillIdList.Select(x => x.ToString()).ConcatToString("/"),
            learnLevelList.Select(x => x.ToString()).ConcatToString("/") };
    }

    private List<Skill> GetSkillList() {
        PetInfo info = Pet.GetPetInfo(id);
        List<int> chain = PetExpSystem.GetEvolveChain(info.basic.baseId, id);
        List<Skill> skills = new List<Skill>();

        for (int i = 0; i < chain.Count; i++) {
            info = Pet.GetPetInfo(chain[i]);
            skills.AddRange(info.skills.skillIdList.Select(id => Skill.GetSkill(id)));
        }

        return skills;
    }

    private List<int> GetLearnLevelList() {
        PetInfo info = Pet.GetPetInfo(id);
        List<int> chain = PetExpSystem.GetEvolveChain(info.basic.baseId, id);
        List<int> levels = new List<int>();

        for (int i = 0; i < chain.Count; i++) {
            info = Pet.GetPetInfo(chain[i]);
            levels.AddRange(info.skills.learnLevelList);
        }
        return levels;
    }

    private List<LearnSkillInfo> GetLearnInfoList() {
        return skillList.Zip(GetLearnLevelList(), (s, l) => new LearnSkillInfo(s, l)).ToList();
    }
}
