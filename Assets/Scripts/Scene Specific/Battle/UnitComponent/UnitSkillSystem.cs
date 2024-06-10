using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitSkillSystem
{   
    public Skill skill = null;
    public float level;
    public float atk;
    public float def;
    public float weatherBuff = 1f;
    public float sameElementBuff = 1f;
    public float elementRelation = 1f;
    public bool isHit = true;
    public bool isCritical = false;


    // damage to op, buff damage to me, heal to me
    public int skillDamage = 0, skillHeal = 0;
    public int itemDamage = 0, itemHeal = 0;
    public int buffDamage = 0, buffHeal = 0;
    public int damage => skillDamage + itemDamage;
    public int heal => skillHeal + itemHeal;

    public UnitSkillSystem() {
        OnTurnStart();
    }

    public UnitSkillSystem(UnitSkillSystem rhs) {
        skill = (rhs.skill == null) ?  null : new Skill(rhs.skill);
        level = rhs.level;
        atk = rhs.atk;
        def = rhs.def;
        weatherBuff = rhs.weatherBuff;
        sameElementBuff = rhs.sameElementBuff;
        elementRelation = rhs.elementRelation;
        isHit = rhs.isHit;
        isCritical = rhs.isCritical;

        skillDamage = rhs.skillDamage;
        buffDamage = rhs.buffDamage;
        itemDamage = rhs.itemDamage;

        skillHeal = rhs.skillHeal;
        buffHeal = rhs.buffHeal;
        itemHeal = rhs.itemHeal;
    }

    public void OnTurnStart() {
        skill = null;

        level = atk = def = 0;
        weatherBuff = sameElementBuff = elementRelation = 1f;
        isHit = true;
        isCritical = false;

        skillDamage = itemDamage = buffDamage = 0;
        skillHeal = itemHeal = buffHeal = 0;
    }

    public bool CalculateAccuracy(BattlePet atkPet, BattlePet defPet) {
        float _random = Random.Range(0f, 100f);
        isHit = (skill.accuracy + atkPet.battleStatus.hit - defPet.battleStatus.eva) >= _random;
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
            return 0;
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
        return id switch {
            "level" => level,
            "atk" => atk,
            "def" => def,
            "weather" => weatherBuff,
            "sameElement" => sameElementBuff,
            "elementRelation" => elementRelation,
            "damage" => skillDamage,
            "hit" => isHit ? 1 : 0,
            "criticalResult" => isCritical ? 1 : 0,
            _ => float.MinValue,
        };
    }

    public bool TryGetSkillSystemIdentifier(string id, out float num) {
        num = GetSkillSystemIdentifier(id);
        return num != float.MinValue;
    }

    public void SetSkillSystemIdentifier(string id, float num) {
        switch (id) {
            default:
                skill.SetSkillIdentifier(id, num);
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
        }
    }

}
