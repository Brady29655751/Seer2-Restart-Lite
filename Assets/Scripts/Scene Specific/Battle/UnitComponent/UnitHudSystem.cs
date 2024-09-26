using UnityEngine;

public class UnitHudSystem
{
    private PetAnimationType _petAnimType = PetAnimationType.Idle;

    public OtherSidePetReactionInfo CurOtherSidePetReactionInfo;

    public DamageInfo CurDamageInfo;

    public CaptureInfo CurCaptureInfo;

    public HealInfo CurHealInfo;

    public UnitHudSystem()
    {
    }

    public PetAnimationType petAnimType
    {
        get => _petAnimType;
        private set => _petAnimType = value;
    }

    public UnitHudSystem(UnitHudSystem rhs)
    {
        this.CurDamageInfo = rhs.CurDamageInfo;
        this.CurCaptureInfo = rhs.CurCaptureInfo;
        this.CurHealInfo = rhs.CurHealInfo;
        // this.CurOtherSidePetReactionInfo = rhs.CurOtherSidePetReactionInfo;
        // this.petAnimType = rhs.petAnimType;
    }

    public virtual void OnTurnStart(BattlePet pet)
    {
        this.CurDamageInfo = null;
        this.CurCaptureInfo = null;
        this.CurHealInfo = null;
        if (pet.isDead)
        {
            this.petAnimType = PetAnimationType.Lose;
        }
        else if ((float)pet.hp / pet.maxHp < 0.2f)
        {
            this.petAnimType = PetAnimationType.Dying;
        }
        else
        {
            this.petAnimType = PetAnimationType.Idle;
        }
    }

    public void OnPassivePetChange(BattlePet pet)
    {
        this.petAnimType = PetAnimationType.Idle;
    }


    public void OnUseItem(Unit thisUnit, Skill atkSkill, bool isAtkUnit)
    {
        if (isAtkUnit)
        {
            bool isApplyHeal = (thisUnit.skillSystem.heal > 0) ||
                               (atkSkill.effects.Exists(x => x.ability == EffectAbility.Heal));
            if (isApplyHeal)
            {
                this.CurHealInfo = new HealInfo(thisUnit.skillSystem.heal, thisUnit.IsMyUnit(),
                    thisUnit.skill.type == SkillType.道具);
            }
        }
        else
        {
            if (atkSkill.isCapture)
            {
                this.CurCaptureInfo = new CaptureInfo(atkSkill.captureAnimType);
            }
        }
    }

    public void OnUseItemUndo()
    {
        this.CurCaptureInfo = null;
        this.CurHealInfo = null;
        this.petAnimType = PetAnimationType.Idle;
    }

    public void OnAttack(Unit thisUnit, bool isAtkUnit)
    {
        if (isAtkUnit)
        {
            this.petAnimType = thisUnit.skill.petAnimType;
        }
    }

    public void OnAttackUndo()
    {
        this.petAnimType = PetAnimationType.Idle;
    }

    public void OnHit(Unit thisUnit, bool isAtkUnit)
    {
        if (isAtkUnit)
        {
            if (thisUnit.skillSystem.skill.isAttack)
            {
                this.CurDamageInfo = new DamageInfo(thisUnit.IsMyUnit(), true, thisUnit.skillSystem.skillDamage,
                    thisUnit.skillSystem.isHit, thisUnit.skillSystem.isCritical, thisUnit.skillSystem.elementRelation);
                this.CurOtherSidePetReactionInfo = new OtherSidePetReactionInfo(this.CurDamageInfo);
            }

            if (thisUnit.skillSystem.heal != 0)
            {
                this.CurHealInfo = new HealInfo(thisUnit.skillSystem.heal, thisUnit.IsMyUnit(),
                    thisUnit.skill.type == SkillType.道具);
            }
        }
    }

    public void OnHitUndo(bool isAtkUnit)
    {
        this.CurDamageInfo = null;
        this.CurCaptureInfo = null;
        this.CurHealInfo = null;
        this.CurOtherSidePetReactionInfo = null;
        this.petAnimType = PetAnimationType.Idle;
    }

    public void OnTurnEnd(Unit thisUnit, Unit rhsUnit)
    {
        int thisResult = thisUnit.skillSystem.buffHeal - thisUnit.skillSystem.buffDamage;
        int rhsResult = rhsUnit.skillSystem.buffHeal - rhsUnit.skillSystem.buffDamage;
        if (rhsResult < 0)
        {
            this.CurDamageInfo = new DamageInfo(thisUnit.IsMyUnit(), false, Mathf.Abs(rhsResult));
        }
        else if (thisResult > 0)
        {
            this.CurHealInfo = new HealInfo(thisResult, thisUnit.IsMyUnit(), thisUnit.skill.type == SkillType.道具);
        }

        if (thisUnit.pet.isDead)
        {
            this.petAnimType = PetAnimationType.Lose;
        }
        else if ((float)thisUnit.pet.hp / thisUnit.pet.maxHp < 0.2f)
        {
            this.petAnimType = PetAnimationType.Dying;
        }
        else
        {
            this.petAnimType = PetAnimationType.Idle;
        }
    }

    public void OnTurnEndUndo()
    {
        this.CurDamageInfo = null;
        this.CurCaptureInfo = null;
        this.CurHealInfo = null;
        this.CurOtherSidePetReactionInfo = null;
    }

    public void OnBattleEnd(bool isWin)
    {
        this.CurDamageInfo = null;
        this.CurCaptureInfo = null;
        this.CurHealInfo = null;
        this.CurOtherSidePetReactionInfo = null;
        this.petAnimType = isWin ? PetAnimationType.Win : PetAnimationType.Lose;
    }


    public class CaptureInfo
    {
        public readonly PetAnimationType CaptureAnimType; //两种, 一种是捕捉成功, 一种是捕捉失败

        public CaptureInfo(PetAnimationType captureAnimType = PetAnimationType.None) //如果不触发捕捉动画.后面的参数都不用填
        {
            this.CaptureAnimType = captureAnimType;
        }
    };

    public class DamageInfo
    {
        public readonly bool IsMe;
        public readonly bool DamageType; //技能伤害ture, buff伤害false
        public readonly int Damage; //伤害值
        public readonly bool IsHit; //是否命中(没有伤害不代表没有命中,也有可能是吸收)
        public readonly bool IsCritical; //是否暴击
        public readonly float ElementRelation; //属性相克关系,决定颜色

        public DamageInfo(bool isMe, bool damageType, int damage, bool isHit = true, bool isCritical = false,
            float elementRelation = 1f)
        {
            this.IsMe = isMe;
            this.DamageType = damageType;
            this.IsCritical = isCritical;
            this.Damage = damage;
            this.IsHit = isHit;
            this.ElementRelation = elementRelation;
        }
    };

    public class OtherSidePetReactionInfo
    {
        public readonly PetAnimationType ReactionAnimType; //对方精灵的反应动画

        public OtherSidePetReactionInfo(DamageInfo damageInfo) //构造器为什么选用DamageInfo?答:对方精灵的反应一定取决于伤害的类型,诸如是否命中或暴击等等
        {
            if (damageInfo.DamageType)
            {
                if (damageInfo.IsHit)
                {
                    this.ReactionAnimType =
                        damageInfo.IsCritical ? PetAnimationType.BeCriticalStruck : PetAnimationType.Hurt;
                }
                else
                {
                    this.ReactionAnimType = PetAnimationType.Evade;
                }
            }
            else
            {
                this.ReactionAnimType = PetAnimationType.Idle;
            }
        }
    };

    public class HealInfo
    {
        public readonly int Heal; //治疗值

        public readonly bool IsMe;

        public readonly bool IsForceShowHeal;

        public HealInfo(int heal, bool isMe, bool isForceShowHeal = false)
        {
            this.Heal = heal;
            this.IsMe = isMe;
            this.IsForceShowHeal = isForceShowHeal;
        }
    };
}