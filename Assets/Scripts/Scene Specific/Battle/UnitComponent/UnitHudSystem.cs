using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitHudSystem
{
    public PetAnimationType petAnimType = PetAnimationType.None;
    public bool applyDamageSystemAnim => GetApplyDamageSystemAnim();
    public bool applyPetAnim = false;
    public bool applySkillBubbleAnim = false;
    public bool applyDamageAnim = false, applyBuffDamageAnim = false;
    public int damage = 0, buffDamage = 0;
    public bool applyHealAnim = false;
    public int heal = 0;

    public UnitHudSystem() {
        OnTurnStart();
    }

    public UnitHudSystem(UnitHudSystem rhs) {
        petAnimType = rhs.petAnimType;
        applyPetAnim = rhs.applyPetAnim;
        applySkillBubbleAnim = rhs.applySkillBubbleAnim;
        applyDamageAnim = rhs.applyDamageAnim;
        applyBuffDamageAnim = rhs.applyBuffDamageAnim;
        damage = rhs.damage;
        buffDamage = rhs.buffDamage;
        applyHealAnim = rhs.applyHealAnim;
        heal = rhs.heal;
    }

    public virtual bool GetApplyDamageSystemAnim() {
        return applyDamageAnim || applyBuffDamageAnim || applyHealAnim;
    }

    public virtual void OnTurnStart() {
        petAnimType = PetAnimationType.None;
        applyPetAnim = false;
        applySkillBubbleAnim = false;
        applyDamageAnim = applyBuffDamageAnim = false;
        applyHealAnim = false;
        damage = buffDamage = heal = 0;
    }

    public void OnUseItem(Unit thisUnit, Skill atkSkill, bool isAtkUnit) {
        if (isAtkUnit) {
            applyHealAnim = (thisUnit.skillSystem.heal > 0) || (atkSkill.effects.Exists(x => x.ability == EffectAbility.Heal));
            heal = thisUnit.skillSystem.heal;
        } else {
            applyPetAnim = atkSkill.isCapture;
            petAnimType = atkSkill.captureAnimType;
        }
    }

    public void OnUseItemUndo() {
        applyHealAnim = false;
        heal = 0;
        applyPetAnim = false;
        petAnimType = PetAnimationType.None;
    }

    public void OnAttack(Unit thisUnit, bool isAtkUnit) {
        if (isAtkUnit) {
            applySkillBubbleAnim = true;
            applyPetAnim = true;
            petAnimType = thisUnit.skill.petAnimType;
        }
    }

    public void OnAttackUndo() {
        applyPetAnim = false;
        petAnimType = PetAnimationType.None;
    }

    public void OnHit(Unit thisUnit, bool isAtkUnit) {
        if (isAtkUnit) {
            applyDamageAnim = thisUnit.skill.isAttack || !thisUnit.skillSystem.isHit;
            damage = thisUnit.skillSystem.skillDamage;
            applyHealAnim = true;
            heal = thisUnit.skillSystem.heal;
        } else {
            applyPetAnim = true;
            petAnimType = (thisUnit.skillSystem.skillDamage != 0) ? PetAnimationType.Hurt : PetAnimationType.Evade;
        }
    }

    public void OnHitUndo(bool isAtkUnit) {
        if (isAtkUnit) {
            applySkillBubbleAnim = false;
            applyDamageAnim = applyHealAnim = false;
            damage = heal = 0;
        } else {
            applyPetAnim = false;
            petAnimType = PetAnimationType.None;
        }
    }

    public void OnTurnEnd(Unit thisUnit, Unit rhsUnit) {
        int thisResult = thisUnit.skillSystem.buffHeal - thisUnit.skillSystem.buffDamage;
        int rhsResult = rhsUnit.skillSystem.buffHeal - rhsUnit.skillSystem.buffDamage;

        applyBuffDamageAnim = (rhsResult < 0);
        buffDamage = applyBuffDamageAnim ? Mathf.Abs(rhsResult) : 0;

        applyHealAnim = (thisResult > 0);
        heal = applyHealAnim ? thisResult : 0;
    }

    public void OnTurnEndUndo() {
        applyBuffDamageAnim = applyHealAnim = false;
        buffDamage = heal = 0;
    }
}
