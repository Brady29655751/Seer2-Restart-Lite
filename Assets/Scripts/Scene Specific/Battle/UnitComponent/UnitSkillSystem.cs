using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class UnitSkillSystem
{   
    public static string[] normalHpType => new string[] { "skill", "item", "buff" };

    public Skill skill = null;
    public Skill counterSkill = null;
    public float level;
    public float atk;
    public float def;
    public float weatherBuff = 1f;
    public float sameElementBuff = 1f;
    public float elementRelation = 1f;
    public bool isCounter = false;
    public bool isHit = true;
    public bool isCritical = false;


    // damage to op, buff damage to me, heal to me.
    public Dictionary<string, int> damageDict = new Dictionary<string, int>();
    public Dictionary<string, int> healDict = new Dictionary<string, int>();

    public int skillDamage = 0, skillHeal = 0;
    public int itemDamage = 0, itemHeal = 0;
    public int buffDamage = 0, buffHeal = 0;

    public int totalSkillDamage => Mathf.Max(skillDamage, 0) + GetTypeDamage(type => type.StartsWith("skill"));
    public int totalItemDamage => Mathf.Max(itemDamage, 0) + GetTypeDamage(type => type.StartsWith("item"));
    public int totalBuffDamage => Mathf.Max(buffDamage, 0) + GetTypeDamage(type => type.StartsWith("buff"));

    public int totalSkillHeal => skillHeal + GetTypeHeal(type => type.StartsWith("skill"));
    public int totalItemHeal => itemHeal + GetTypeHeal(type => type.StartsWith("item"));
    public int totalBuffHeal => buffHeal + GetTypeHeal(type => type.StartsWith("buff"));
    
    public int damage => totalSkillDamage + totalItemDamage;
    public int heal => totalSkillHeal + totalItemHeal;

    public UnitSkillSystem() {
        OnTurnStart();
    }

    public UnitSkillSystem(UnitSkillSystem rhs) {
        skill = (rhs.skill == null) ?  null : new Skill(rhs.skill);
        counterSkill = (rhs.counterSkill == null) ?  null : new Skill(rhs.counterSkill);
        level = rhs.level;
        atk = rhs.atk;
        def = rhs.def;
        weatherBuff = rhs.weatherBuff;
        sameElementBuff = rhs.sameElementBuff;
        elementRelation = rhs.elementRelation;
        isCounter = rhs.isCounter;
        isHit = rhs.isHit;
        isCritical = rhs.isCritical;

        damageDict = new Dictionary<string, int>(rhs.damageDict);
        healDict = new Dictionary<string, int>(rhs.healDict);

        skillDamage = rhs.skillDamage;
        buffDamage = rhs.buffDamage;
        itemDamage = rhs.itemDamage;

        skillHeal = rhs.skillHeal;
        buffHeal = rhs.buffHeal;
        itemHeal = rhs.itemHeal;
    }

    public Skill GetSkillByKey(string key)
    {
        return key switch
        {
            "counter" => counterSkill,
            _ => skill,
        };
    }

    public void EnsureSkillNotNull()
    {
        skill ??= Skill.GetNoOpSkill();
        counterSkill ??= Skill.GetNoOpSkill();
    }

    public int GetTypeDamage(Func<string, bool> filter)
    {
        return damageDict.Where(entry => filter(entry.Key)).Sum(entry => Mathf.Max(entry.Value, 0));
    }

    public int GetTypeHeal(Func<string, bool> filter)
    {
        return healDict.Where(entry => filter(entry.Key)).Sum(entry => entry.Value);
    }
    
    public void OnTurnStart() {
        skill = null;
        counterSkill = null;

        level = atk = def = 0;
        weatherBuff = sameElementBuff = elementRelation = 1f;
        isCounter = false;
        isHit = true;
        isCritical = false;

        damageDict.Clear();
        healDict.Clear();

        skillDamage = itemDamage = buffDamage = 0;
        skillHeal = itemHeal = buffHeal = 0;
    }

    public void OnChainStart() {
        level = atk = def = 0;
        weatherBuff = sameElementBuff = elementRelation = 1f;
        isHit = true;
        isCritical = false;
        
        skillDamage = skillHeal = 0;
        damageDict = damageDict.Where(x => !x.Key.StartsWith("skill")).ToDictionary(x => x.Key, x => x.Value);
        healDict = healDict.Where(x => !x.Key.StartsWith("skill")).ToDictionary(x => x.Key, x => x.Value);

        if (!Skill.IsNullOrEmpty(skill) && !skill.isAction)
        {
            var testSkill = new Skill(Skill.GetSkill(skill.id));
            skill.critical = testSkill.critical;
            skill.combo = testSkill.combo;
            skill.accuracy = testSkill.accuracy;
            skill.power = testSkill.power;
        }
    }

    public void SwapCounterSkill(bool isCounterStart)
    {
        isCounter = isCounterStart;
        
        var tmp = counterSkill;
        counterSkill = isCounterStart ? skill : Skill.GetNoOpSkill();
        skill = tmp;
    }

    public bool CalculateAccuracy(BattlePet atkPet, BattlePet defPet)
    {
        float _random = Random.Range(0f, 100f);
        var accuracyBuff = atkPet.statusController.GetCurrentPowerup(skill.powerup, skill.powerdown).accuracyBuff;
        var accuracy = skill.accuracy + accuracyBuff + atkPet.battleStatus.hit - defPet.battleStatus.eva;

        if (skill.options.TryGet("accuracy", out var finalAccuracy))
            accuracy = Identifier.GetNumIdentifier(finalAccuracy);

        isHit = accuracy >= _random;
        return isHit;
    }

    public void PrepareDamageParam(BattlePet atkPet, BattlePet defPet) {
        if (skill == null)
            return;

        level = (2 * atkPet.level + 10) / 250f;
        var status = BattlePet.GetSkillTypeStatus(skill, atkPet, defPet);
        atk = Mathf.Max(status.Key, 1);
        def = Mathf.Max(status.Value, 1);
        elementRelation = PetElementSystem.GetElementRelation(skill, defPet);
        sameElementBuff = ((skill.element == atkPet.battleElement) || ((skill.element == atkPet.subBattleElement) &&
            (atkPet.subBattleElement != Element.普通))) ? 1.5f : 1f;
    }

    public int CalculateDamage(BattlePet atkPet, BattlePet defPet) {
        if (skill == null) {
            return skillDamage;
        }

        for (int i = 0; i < skill.combo; i++) {
            float _random = Random.Range(0.85f, 1f);
            float _criRandom = Random.Range(0f, 100f);
            bool _isCritical = (skill.critical + atkPet.battleStatus.cri - defPet.battleStatus.cdf) >= _criRandom;
            float _damage = (level * (atk / def) * skill.power + 2) 
                * sameElementBuff * elementRelation * weatherBuff * _random * 
                (_isCritical ? 2 : 1);

            skillDamage += (int)Mathf.Ceil(_damage);
            isCritical |= _isCritical;
        }
        
        return skillDamage;
    }

    public float GetSkillSystemIdentifier(string id) {
        if (id.TryTrimStart("damage", out var trimId) && 
            trimId.TryTrimParentheses(out var damageType)) 
        {
            return damageType switch {
                "item" => totalItemDamage,
                "buff" => totalBuffDamage,
                "skill" => skillDamage,
                _ => damageDict.Get(damageType, 0),
            };
        }

        if (id.TryTrimStart("heal", out trimId) && 
            trimId.TryTrimParentheses(out var healType)) 
        {
            return healType switch {
                "item" => totalItemHeal,
                "buff" => totalBuffHeal,
                "skill" => skillHeal,
                _ => healDict.Get(healType, 0),
            };
        }

        return id switch {
            "level" => level,
            "atk" => atk,
            "def" => def,
            "weather" => weatherBuff,
            "sameElement" => sameElementBuff,
            "elementRelation" => elementRelation,
            "damage" => totalSkillDamage,
            "heal" => totalSkillHeal,
            "counter" => isCounter ? 1 : 0,
            "hit" => isHit ? 1 : 0,
            "criticalResult" => isCritical ? 1 : 0,
            _ => float.MinValue,
        };
    }

    public bool TryGetSkillSystemIdentifier(string id, out float num) {
        num = GetSkillSystemIdentifier(id);
        return num != float.MinValue;
    }

    public void SetSkillSystemIdentifier(string id, float num, string key = null) {
        switch (id)
        {
            default:
                GetSkillByKey(key).SetSkillIdentifier(id, num);
                return;
            case "level":
                level = num;
                return;
            case "atk":
                atk = num;
                return;
            case "def":
                def = num;
                return;
            case "weather":
                weatherBuff = num;
                return;
            case "sameElement":
                sameElementBuff = num;
                return;
            case "elementRelation":
                elementRelation = num;
                return;
            case "damage":
                skillDamage = (int)num;
                return;
            case "heal":
                skillHeal = (int)num;
                return;
        }
    }

}
