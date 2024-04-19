using System;
using System.Xml.Serialization;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using System.Threading.Tasks;

[XmlRoot("pet")]
public class Pet
{
    [XmlIgnore] public PetInfo info => GetPetInfo(id);

    public int id;
    public PetBasic basic;   // 基本資料，屬性、性格、身高體重、獲得日期等
    public PetFeature feature;  // 特性、紋章等
    public PetExp exp;  // 經驗值、等級相關等
    public PetTalent talent;    // 個體值、學習力等
    public PetSkill skills;     // 技能相關等
    public PetRecord record;   // 精靈紀錄，通常為隱藏技能條件判斷用
    public PetUI ui;  // 皮膚、頭像等UI資源

    public Status currentStatus = new Status();   // 當前能力值
    public Status normalStatus => Status.GetPetNormalStatus(  // 正常狀態下的能力值
        level, info.basic.baseStatus, talent.iv, talent.ev, basic.personality);



    #region properties
    // Properties and Shortcut.
    /* Basic */
    public string name => info.basic.name;

    /* Feature */
    public int elementId => info.basic.elementId;
    public Element element => info.basic.element;  // 屬性
    public bool hasEmblem => feature.hasEmblem;   // 是否佩戴紋章

    /* Exp and Level */
    public int level => exp.level;  // 等級
    public uint totalExp => exp.totalExp;   // 目前總計獲得EXP
    public uint levelUpExp => exp.levelUpExp;   // 距離升級所需EXP
    public uint maxExp => exp.maxExp;   // 100級所需EXP

    /* Skill */
    [XmlIgnore] public List<Skill> ownSkill => skills.ownSkill;  // 當前習得的所有技能
    [XmlIgnore] public Skill[] normalSkill {    // 配備的四個普通技能
        get => skills.normalSkill; 
        set => skills.normalSkill = value;
    }
    [XmlIgnore] public Skill superSkill {   // 必殺技
        get => skills.superSkill;
        set => skills.superSkill = value;
    }
    public Skill[] backupNormalSkill => skills.backupNormalSkill;   // 未配備但已習得的普通技能
    public Skill backupSuperSkill => skills.backupSuperSkill;   // 未配備但已習得的必殺技

    #endregion

    public static PetInfo GetPetInfo(int id) {
        return Database.instance.GetPetInfo(id);
    }

    public static Pet GetExamplePet(int id) {
        PetInfo info = GetPetInfo(id);
        if (info == null)
            return null;

        Pet pet = new Pet(id);

        pet.basic.height = info.basic.baseHeight + 5;
        pet.basic.weight = info.basic.baseWeight + 5;
        pet.basic.ToBestPersonality();

        pet.exp.level = 100;
        pet.exp.totalExp = pet.maxExp;

        pet.feature.hasEmblem = true;

        pet.talent.AddEVStorage(510);
        pet.talent.iv = 31;

        pet.skills.LearnAllSkill();

        pet.currentStatus = pet.normalStatus;

        return pet;
    }

    public static Pet ToBestPet(Pet pet) {
        Pet bestPet = (pet.level < 100) ? pet.GainExp(pet.maxExp) : pet;
        PetInfo info = bestPet.info;

        bestPet.feature.hasEmblem = true;

        bestPet.talent.AddEVStorage(510);
        bestPet.talent.iv = 31;

        var unlearnedSkills = info.skills.skillList.Where(x => !bestPet.skills.ownSkillId.Contains(x.id));
        foreach (var skill in unlearnedSkills) {
            bestPet.skills.LearnNewSkill(skill);
        }
        return bestPet;
    }

    public static void Add(Pet pet) {
        int index = Player.instance.petBag.IndexOf(null);
        if (index == -1) {
            Player.instance.gameData.petStorage.Add(pet);
            return;
        }
        Player.instance.petBag[index] = pet;
    }

    public Pet() {}

    public Pet(Pet _copy) {
        if (_copy == null)
            return;

        id = _copy.id;

        basic = new PetBasic(_copy.basic);
        feature = new PetFeature(_copy.feature);
        exp = new PetExp(_copy.exp);
        talent = new PetTalent(_copy.talent);
        skills = new PetSkill(_copy.skills);
        record = new PetRecord(_copy.record);
        ui = new PetUI(_copy.ui);

        currentStatus = new Status(_copy.currentStatus);
    }

    public Pet(int petId, int initLevel = 1, bool hasEmblem = true) {
        id = petId;
        
        basic = new PetBasic(id);  
        feature = new PetFeature(id, hasEmblem);  
        exp = new PetExp(id, initLevel);
        talent = new PetTalent(id);
        skills = new PetSkill(id, initLevel);
        record = new PetRecord();
        ui = new PetUI(info.ui.defaultSkinId, info.ui.baseId);

        /* Status */
        currentStatus = new Status(normalStatus);
    }

    public Pet(int evolvePetId, Pet originalPet) {
        /* Basic */
        id = evolvePetId;

        basic = new PetBasic(evolvePetId, (int)originalPet.basic.personality);
        feature = new PetFeature(evolvePetId, originalPet.feature.hasEmblem);
        exp = new PetExp(evolvePetId, originalPet.level, originalPet.totalExp);
        talent = new PetTalent(evolvePetId, originalPet.talent);
        skills = new PetSkill(evolvePetId, originalPet.level, originalPet.skills);
        record = new PetRecord(originalPet.record);
        ui = new PetUI(info.ui.defaultSkinId, info.ui.baseId);

        /* Status */
        currentStatus = new Status(normalStatus);
    }

    public override string ToString()
    {
        return "id: " + id.ToString() + " name: " + name;
    }

    public float GetPetIdentifier(string id) {
        return id switch {
            "id" => this.id,
            "baseId" => basic.baseId,
            "personality" => (float)basic.personality,
            "height" => basic.height,
            "weight" => basic.weight,
            "level" => level,
            "exp" => totalExp,
            "levelExp" => levelUpExp,
            "iv" => talent.iv, 
            "evStorage" => talent.evStorage,
            "emblem" => hasEmblem ? 1 : 0,
            _ => float.MinValue
        };
    }

    public bool TryGetPetIdentifier(string id, out float num) {
        num = GetPetIdentifier(id);
        return num != float.MinValue;
    }

    public void SetPetIdentifier(string id, float num) {
        switch (id) {
            default:
                return;
            case "personality":
                basic.personality = (Personality)num;
                currentStatus = new Status(normalStatus){ hp = currentStatus.hp };
                return;
            case "exp":
                uint gainExp = (uint)num - totalExp;
                GainExp(gainExp);
                return;
            case "iv":
                talent.iv = (int)num;
                currentStatus = new Status(normalStatus){ hp = Mathf.Min(normalStatus.hp, currentStatus.hp) };
                return;
            case "evStorage":
                talent.SetEVStorage((int)num);
                return;
            case "emblem":
                feature.hasEmblem = (num > 0);
                return;
            case "height":
                basic.height = ((int)num);
                return;
            case "weight":
                basic.weight = ((int)num);
                return;
        }
    }

    public Pet GainExp(uint gainExp) {
        bool isLevelUp = (gainExp >= levelUpExp);
        if (level >= 100)
            return this;
            
        if (exp.GainExp(gainExp)) {
            Pet pet = Evolve();
            SaveSystem.SaveData();
            return pet;
        }
        if (isLevelUp) {
            currentStatus = new Status(normalStatus);
            skills.CheckNewSkill(level);
        }
        SaveSystem.SaveData();
        return this;
    }

    public Pet Evolve() {
        int cursor = Player.instance.petBag.AllIndexOf(this).FirstOrDefault();

        var evolveInfo = info;
        int evolveLevel = info.exp.evolveLevel;
        while ((evolveInfo.exp.evolvePetId != 0) && (evolveLevel != 0) && (level >= evolveLevel)) {
            evolveInfo = GetPetInfo(evolveInfo.exp.evolvePetId);
            evolveLevel = evolveInfo.exp.evolveLevel;
        }
        Pet evolvePet = new Pet(evolveInfo.id, this);
        Player.instance.petBag[cursor] = evolvePet;
        return evolvePet;
    }

    public Pet MegaEvolve(int evolveId) {
        int cursor = Player.instance.petBag.AllIndexOf(this).FirstOrDefault();
        if (evolveId == 0)
            return null;

        Pet evolvePet = new Pet(evolveId, this);
        Player.instance.petBag[cursor] = evolvePet;
        return evolvePet;
    }

    public static void VersionUpdate() {
        GameData gameData = Player.instance.gameData;
        string petDataVersion = gameData.version;
        if (VersionData.Compare(petDataVersion, "beta_0.1") < 0) {
            var allPets = gameData.petBag.Concat(gameData.petStorage);
            foreach (var pet in allPets) {
                pet.ui = new PetUI(pet.id, pet.basic.baseId);
            }
            petDataVersion = "beta_0.1";
        }
    }

}

