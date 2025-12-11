using System;
using System.Xml.Serialization;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using System.Threading.Tasks;
using ExitGames.Client.Photon.StructWrapping;

[XmlRoot("pet")]
public class Pet
{
    [XmlIgnore] public PetInfo info => GetPetInfo(id);

    public int id;
    public Pet backupPet;  // 原始型態（退化時將退化前的型態放在這）
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
    public Status extraStatus => feature.GetExtraStatus(normalStatus);   // 後天加成等額外的能力值


    #region properties
    // Properties and Shortcut.
    /* Basic */
    public int hashId => GetPetHashId();
    [XmlIgnore] public string name
    {
        get => string.IsNullOrEmpty(basic.name) ? info.basic.name : basic.name;
        set => basic.name = value;
    }

    /* Feature */
    public int elementId => info.basic.elementId;
    public Element element => info.basic.element;  // 屬性
    public int subElementId => info.basic.subElementId;
    public Element subElement => info.basic.subElement; // 副屬性
    public bool hasEmblem => feature.hasEmblem;   // 是否佩戴紋章
    [XmlIgnore] public List<Buff> initBuffs => info.ui.defaultBuffs.Concat(feature.afterwardBuffs).ToList();

    /* Exp and Level */
    public int level => exp.level;  // 等級
    public int maxLevel => exp.maxLevel;    // 最高等級
    public uint totalExp => exp.totalExp;   // 目前總計獲得EXP
    public uint levelUpExp => exp.levelUpExp;   // 距離升級所需EXP
    public uint maxExp => exp.maxExp;   // 滿級所需EXP

    /* Skill */
    [XmlIgnore] public List<Skill> ownSkill {     // 當前習得的所有技能
        get => skills.ownSkill;
        set => skills.ownSkill = value;
    }
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

    public static PetInfo GetRandomPetInfo(bool withMod = false) {
        IEnumerable<KeyValuePair<int, PetInfo>> petInfoDict = Database.instance.petInfoDict;
        if (!withMod) 
            petInfoDict = petInfoDict.Where(entry => !PetInfo.IsMod(entry.Key));
        
        return petInfoDict.Select(entry => entry.Value).ToList().Random();
    }

    public static Pet GetExamplePet(int id, int level = 100, int iv = 31) {
        PetInfo info = GetPetInfo(id);
        if (info == null)
            return null;

        Pet pet = new Pet(id);

        pet.basic.gender = info.basic.gender;
        pet.basic.height = info.basic.baseHeight + 5;
        pet.basic.weight = info.basic.baseWeight + 5;
        pet.basic.ToBestPersonality();

        pet.exp.level = level;
        pet.exp.totalExp = PetExpSystem.GetTotalExp(level, pet.exp.expType);

        pet.feature.hasEmblem = true;

        pet.talent.AddEVStorage(510);
        pet.talent.iv = iv;

        pet.skills.LearnAllSkill();

        pet.currentStatus = pet.normalStatus;

        return pet;
    }

    public static Pet ToBestPet(Pet pet, int iv = 31) {
        Pet bestPet = (pet.level < 100) ? pet.GainExp(pet.maxExp) : pet;
        PetInfo info = bestPet.info;

        bestPet.feature.hasEmblem = true;

        bestPet.talent.AddEVStorage(510);
        bestPet.talent.iv = iv;

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

    public static ItemHintbox OpenHintbox(Pet pet)
    {
        if (pet == null)
            return null;

        var itemHintbox = Hintbox.OpenHintbox<ItemHintbox>();
        itemHintbox.SetIcon(pet.ui.icon);
        itemHintbox.SetContent($"获得了【{pet.name}】！", 16, FontOption.Arial);
        itemHintbox.SetOptionNum(1);
        return itemHintbox;
    }

    public Pet() {}

    public Pet(Pet _copy) {
        if (_copy == null)
            return;

        id = _copy.id;
        backupPet = (_copy.backupPet == null) ? null : new Pet(_copy.backupPet);

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

        basic = new PetBasic(evolvePetId, (int)originalPet.basic.personality, originalPet.basic.gender)
        {
            getPetDate = originalPet.basic.getPetDate,
        };
        feature = new PetFeature(evolvePetId, originalPet.feature.hasEmblem, 
            originalPet.feature.featureId, originalPet.feature.emblemId, originalPet.feature.afterwardBuffIds);
        exp = new PetExp(evolvePetId, originalPet.level, originalPet.totalExp);
        talent = new PetTalent(evolvePetId, originalPet.talent);
        skills = new PetSkill(evolvePetId, originalPet.level, originalPet.skills);
        record = new PetRecord(originalPet.record);
        ui = new PetUI(info.ui.defaultSkinId, info.ui.baseId, originalPet.ui.specialSkinList);

        /* Status */
        currentStatus = new Status(normalStatus);
    }

    public override string ToString()
    {
        return "id: " + id.ToString() + " name: " + name;
    }

    public int GetPetHashId() {
        unchecked {
            int hash = 17;
            hash = hash * 31 + basic.hashId;
            hash = hash * 31 + ui.hashId;
            return hash;
        };
    }

    public virtual float GetPetIdentifier(string id) {
        if (id.TryTrimStart("status.", out var trimStatus)) {
            trimStatus = trimStatus.ToLower();
            if (trimStatus.TryTrimStart("init", out var trimInitStatus))
                return normalStatus[trimInitStatus];

            if (trimStatus.TryTrimStart("extra", out var trimExtraStatus))
                return extraStatus[trimExtraStatus];

            if (trimStatus.TryTrimStart("battle", out var trimBattleStatus))
                return (normalStatus + extraStatus)[trimBattleStatus];

            return currentStatus[trimStatus];
        }

        if (id.TryTrimStart("ev.", out trimStatus)) {
            trimStatus = trimStatus.ToLower();
            return talent.ev[trimStatus];
        }

        if (id.TryTrimStart("normalSkill", out var trimNormalSkill) &&
            trimNormalSkill.TryTrimParentheses(out var skillIndexExpr) && 
            int.TryParse(skillIndexExpr, out var skillIndex)) {
            var trimId = trimNormalSkill.TrimStart($"[{skillIndexExpr}]").TrimStart('.');
            if (string.IsNullOrEmpty(trimId))
                trimId = "id";

            return normalSkill[skillIndex]?.GetSkillIdentifier(trimId) ?? 0;
        }

        if (id.TryTrimStart("superSkill", out var trimSuperSkill)) {
            var trimId = trimNormalSkill.TrimStart('.');
            if (string.IsNullOrEmpty(trimId))
                trimId = "id";

            return superSkill?.GetSkillIdentifier(trimId) ?? 0;
        }
            

        if ((id.TryTrimStart("skill", out var trimSkill)) && 
            (trimSkill.TryTrimParentheses(out var skillIdExpr)) &&
            (int.TryParse(skillIdExpr, out var skillId))) {
            return skills.ownSkillId.Contains(skillId) ? 1 : 0;
        }

        if ((id.TryTrimStart("buff", out var trimBuff))) {
            bool correctExpr = trimBuff.TryTrimParentheses(out var buffIdExpr);
            if (!correctExpr)
                return float.MinValue;

            if (int.TryParse(buffIdExpr, out var buffId))
                return initBuffs.Exists(x => x.id == buffId) ? 1 : 0;

            var buffOptionExpr = buffIdExpr.Split(':');
            return initBuffs.Count(x => x.options.Get(buffOptionExpr[0], x.info.options.Get(buffOptionExpr[0]))?.ToLower() == buffOptionExpr[1]?.ToLower());
        }

        if ((id.TryTrimStart("record", out var trimRecord)) && 
            (trimRecord.TryTrimParentheses(out var trimRecordKey)) && 
            (record.TryGetRecord(trimRecord, out var trimRecordValueExpr)) &&
            float.TryParse(trimRecordValueExpr, out var trimRecordValue))
            return trimRecordValue;

        if (id.TryTrimStart("skin", out var trimSkin) &&
            (trimSkin.TryTrimParentheses(out var skinIdExpr)) && 
            (int.TryParse(skinIdExpr, out var skinId))) {
            return (info.ui.GetAllSkinList(ui).Contains(skinId) || ui.specialSkinList.Contains(skinId)) ? 1 : 0;
        } 

        if (id.TryTrimStart("baseId", out var trimBase) && 
            (trimBase.TryTrimParentheses(out var trimBaseExpr)) &&
            int.TryParse(trimBaseExpr, out var baseId)) {
            return ListHelper.IsNullOrEmpty(PetExpSystem.GetEvolveChain(baseId, this.id)) ? 0 : 1;
        }
        
        if (id.TryTrimStart("gender", out var trimGender) &&
            (trimSkin.TryTrimParentheses(out var genderIdExpr)) && 
            (int.TryParse(genderIdExpr, out var genderId))) {
            return info.basic.genderList.Contains(genderId) ? 1 : 0;
        }

        if (id == "own")
        {
            var allPets = Player.instance.gameData.petDict.GroupBy(x => x.id).Select(x => x.First()).ToList();
            return allPets.Exists(x => (x.id == this.id) || (x.GetPetIdentifier($"baseId[{this.id}]") == 1)) ? 1 : 0;
        }

        return id switch
        {
            "uid" => this.id,
            "id" => info.ui.defaultId,
            "subId" => info.ui.subId,
            "baseId" => basic.baseId,
            "star" => info.ui.star,
            "skinId" => ui.skinId,
            "element" => elementId,
            "subElement" => subElementId,
            "gender" => (float)basic.gender,
            "personality" => (float)basic.personality,
            "trait" => feature.afterwardBuffs.Find(x => x.info.options.Get("group") == "trait")?.id ?? 0,
            "height" => basic.height,
            "weight" => basic.weight,
            "level" => level,
            "maxLevel" => maxLevel,
            "exp" => totalExp,
            "levelExp" => levelUpExp,
            "iv" => talent.iv,
            "evStorage" => talent.evStorage,
            "emblem" => hasEmblem ? 1 : 0,
            "featureId" => feature.feature.baseId,
            "featureType" => info.ui.defaultFeatureList.IndexOf(feature.feature.baseId),
            "emblemId" => feature.emblem.baseId,
            "emblemType" => info.ui.defaultFeatureList.IndexOf(feature.emblem.baseId),
            _ => float.MinValue
        };
    }

    public virtual bool TryGetPetIdentifier(string id, out float num) {
        num = GetPetIdentifier(id);
        return num != float.MinValue;
    }

    public virtual void SetPetIdentifier(string id, float num) {
        if ((id.TryTrimStart("record", out var trimRecord)) && 
            (trimRecord.TryTrimParentheses(out var trimRecordKey))) {
            record.SetRecord(trimRecordKey, num);
            return;
        }        

        switch (id) {
            default:
                return;
            case "skinId":
                ui.skinId = (int)num;
                return;
            case "skill":
                skills.LearnNewSkill(Skill.GetSkill((int)num, false));
                return;
            case "gender":
                basic.gender = (int)num;
                return;
            case "personality":
                basic.personality = (Personality)num;
                currentStatus = new Status(normalStatus){ hp = currentStatus.hp };
                return;
            case "trait":
                feature.SetTrait((int)num);
                return;
            case "level":
                int toLevel = (int)num;
                if (toLevel <= level)
                    LevelDown(toLevel);
                else
                    GainExp(PetExpSystem.GetTotalExp(Mathf.Min(toLevel, exp.maxLevel), exp.expType) - exp.totalExp);
                return;
            case "maxLevel":
                exp.fixedMaxLevel = Mathf.Max((int)num, 0);
                if (maxLevel < level)
                    LevelDown(maxLevel, true);
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
            case "height":
                basic.height = ((int)num);
                return;
            case "weight":
                basic.weight = ((int)num);
                return;
            case "emblem":
                feature.hasEmblem = (num > 0);
                return;
            case "featureId":
                feature.featureId = (int)num;
                return;
            case "featureType":
                int featureType = (int)num;
                var featureList = info.ui.defaultFeatureList;
                feature.featureId = featureList[featureType % featureList.Count];
                return;
            case "emblemId":
                feature.emblemId = (int)num;
                return;
            case "emblemType":
                int emblemType = (int)num;
                var emblemList = info.ui.defaultFeatureList;
                feature.emblemId = info.ui.defaultFeatureList[emblemType % emblemList.Count];
                return;
        }
    }

    public Pet GainExp(uint gainExp, bool healWhenLevelUp = true) {
        bool isLevelUp = (gainExp >= levelUpExp);
        if (level >= maxLevel)
            return this;
            
        if (exp.GainExp(gainExp)) {
            Pet pet = Evolve();
            if (pet != this) {
                SaveSystem.SaveData();
                return pet;
            }
        }
        if (isLevelUp) {
            var hp = currentStatus.hp;
            currentStatus = new Status(normalStatus);
            if (!healWhenLevelUp)
                currentStatus.hp = hp;
            skills.CheckNewSkill(level);
        }
        SaveSystem.SaveData();
        return this;
    }

    public void LevelDown(int toWhichLevel, bool keepSkill = false) {
        if (level < 1)
            return;

        exp.LevelDown(toWhichLevel);
        if (!keepSkill) {
            skills.ownSkill = null;
            skills.normalSkill = null;
            skills.superSkill = null;
            skills.CheckNewSkill(toWhichLevel);
        }

        currentStatus = normalStatus;
        SaveSystem.SaveData();
    }

    public Pet Evolve() {
        int cursor = Player.instance.petBag.IndexOf(this);
        var evolveInfo = info;
        int evolveLevel = info.exp.evolveLevel;

        if (record.TryGetRecord("evolveBan", out var evolveBanExpr) && 
            int.TryParse(evolveBanExpr, out var evolveBan) && (evolveBan == 1))
            return this;

        while ((evolveInfo.exp.evolvePetId != 0) && (evolveLevel != 0) && (level >= evolveLevel)) {
            evolveInfo = GetPetInfo(evolveInfo.exp.evolvePetId);
            evolveLevel = evolveInfo.exp.evolveLevel;
        }
        Pet evolvePet = new Pet(evolveInfo.id, this);
        if (cursor.IsInRange(0, Player.instance.petBag.Length))
            Player.instance.petBag[cursor] = evolvePet;
        return evolvePet;
    }

    public Pet EvolveTo(int evolveId, bool keepSkill = true) {
        var allPets = Player.instance.petBag.AllIndexOf(this);
        int cursor = ListHelper.IsNullOrEmpty(allPets) ? -1 : allPets[0];
        if (evolveId == 0)
            return null;

        var specialSkills = skills.ownSkill.Where(x => (!skills.skillList.Exists(y => y.id == x.id)) || skills.secretSkillInfo.Any(y => (y.skill.id == x.id) && (y.secretType == SecretType.Others))).ToList();
        Pet evolvePet = new Pet(evolveId, this);
        if (!keepSkill) {
            evolvePet.LevelDown(evolvePet.level);
            specialSkills.ForEach(skill => evolvePet.skills.LearnNewSkill(skill));
        }

        if (cursor.IsInRange(0, Player.instance.petBag.Length))
            Player.instance.petBag[cursor] = evolvePet;
            
        return evolvePet;
    }

    public bool Devolve()
    {
        var allPets = Player.instance.petBag.AllIndexOf(this);
        int cursor = ListHelper.IsNullOrEmpty(allPets) ? -1 : allPets[0];
        if (!cursor.IsInRange(0, Player.instance.petBag.Length))
            return false;

        if (backupPet != null)
        {
            Player.instance.petBag[cursor] = backupPet;
            return true;
        }

        // Get devolve candidates
        var chain = PetExpSystem.GetEvolveChain(basic.baseId, backupPet?.id ?? id);

        // Remove self
        chain?.Remove(id);

        if (ListHelper.IsNullOrEmpty(chain))
        {
            Hintbox.OpenHintboxWithContent("没有可退化的形态", 16);
            return false;
        }

        var starPets = chain.Select(x => new Pet(x, this)).ToList();
        foreach (var starPet in starPets)
        {
            starPet.skills = new PetSkill(starPet.id, starPet.level);
            starPet.skills.LearnAllSkill();
            starPet.backupPet = this;
        }

        var hintbox = Hintbox.OpenHintbox<PetSelectHintbox>();
        hintbox.SetTitle("请选择要退化的形态");
        hintbox.SetStorage(starPets);
        hintbox.SetConfirmSelectCallback(pet =>
        {
            Player.instance.petBag[cursor] = pet;

            var petBagController = GameObject.FindObjectOfType<PetBagController>();
            if (petBagController != null)
                petBagController.RefreshPetBag();
        });
        return true;
    }

    public static void VersionUpdate()
    {
        GameData gameData = Player.instance.gameData;
        string petDataVersion = gameData.version;

        IEnumerable<Pet> allPets = gameData.petBag.Concat(gameData.petStorage).Where(x => x != null);

        if (VersionData.Compare(petDataVersion, "beta_0.1") < 0)
        {
            foreach (var pet in allPets)
                pet.ui = new PetUI(pet.id, pet.basic.baseId);

            petDataVersion = "beta_0.1";
        }

        if (VersionData.Compare(petDataVersion, "lite_2.8") < 0)
        {
            foreach (var pet in allPets)
                pet.basic.gender = pet.info?.basic.gender ?? 0;

            petDataVersion = "lite_2.8";
        }

        if (VersionData.Compare(petDataVersion, "lite_2.9") < 0)
        {
            foreach (var pet in allPets)
            {
                if ((pet == null) || (!pet.id.IsWithin(-1, -12)))
                    return;

                int newId = pet.id switch
                {
                    -1 => 10304,
                    -2 => 10305,
                    -3 => 10306,
                    -4 => 10307,
                    -5 => 10308,
                    -6 => 10309,
                    -7 => 10301,
                    -8 => 10302,
                    -9 => 10303,
                    -10 => 10010,
                    -11 => 10011,
                    -12 => 10012,
                    _ => pet.id
                };

                pet.id = newId;
                pet.basic.id = newId;
                pet.exp.id = newId;
                pet.feature.id = newId;
                pet.talent.id = newId;
                pet.skills.id = newId;
                pet.ui.id = newId;
                pet.ui.baseId = pet.basic.baseId;
            }
            ;

            petDataVersion = "lite_2.9";
        }

        if (VersionData.Compare(petDataVersion, "lite_2.9.4") < 0)
        {
            foreach (var pet in allPets)
                if (!ListHelper.IsNullOrEmpty(PetExpSystem.GetEvolveChain(685, pet.id)))
                    Enumerable.Range(11748, 4).Select(x => Skill.GetSkill(x, false)).ToList().ForEach(x => pet.skills.LearnNewSkill(x));

            petDataVersion = "lite_2.9.4";
        }

        // Check New Skill
        foreach (var pet in allPets)
            pet?.skills?.CheckNewSkill(pet?.level ?? 0);
    }

}

