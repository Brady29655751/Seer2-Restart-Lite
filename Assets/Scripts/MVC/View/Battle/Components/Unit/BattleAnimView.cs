using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleAnimView : BattleBaseView
{
    // Note that the damage view is on enemy's side.
    [SerializeField] private BattlePetAnimView petView;
    [SerializeField] private BattleSkillBubbleAnimView skillBubbleView;
    [SerializeField] private BattleDamageAnimView damageView;
    [SerializeField] private BattlePetAnimView otherSidePetView;

    public bool isDone => petView.isDone;

    public void SetUnit(Unit lastUnit, Unit currentUnit, int cursorOffset = 0)
    {
        if (currentUnit == null)
            return;

        SetPet(lastUnit, currentUnit, cursorOffset);
        SetDamage(currentUnit);
        SetOtherSidePet(currentUnit);
        SetCapture(currentUnit);
        SetHeal(currentUnit);
    }

    private int lastPetId = 0; //上一个单位的宠物id.专门用来判断是否需要刷新宠物

    private void SetPet(Unit lastUnit, Unit currentUnit, int cursorOffset = 0)
    {
        // int lastCursor = (lastUnit == null) ? -1 : lastUnit.petSystem.cursor;
        var currentPet = currentUnit?.petSystem.GetParallelPetBag(battle.settings.parallelCount)?.Get(cursorOffset);
        int currentPetId = currentPet?.hashId ?? 0;
        if (lastPetId != currentPetId)
        {
            petView.SetPet(currentPet, cursorOffset);
            this.lastPetId = currentPetId;
            return;
        }

        if (currentUnit.hudSystem.petAnimType is PetAnimationType.Dying or PetAnimationType.Lose
            or PetAnimationType.Win)
        {
            petView.SetPetStateAnim(currentUnit.hudSystem.petAnimType);
            return;
        }

        if (cursorOffset > 0)
            return;

        if (currentUnit.hudSystem.petAnimType is PetAnimationType.Physic or PetAnimationType.Special
            or PetAnimationType.Property or PetAnimationType.Super or PetAnimationType.SecondSuper)
        {
            skillBubbleView.SetSkill(currentUnit.skill);
            petView.SetPetSkillAnim(currentUnit.skill, currentUnit.hudSystem.petAnimType);
        }
        // petView.SetField(currentUnit.pet);
    }

    private void SetCapture(Unit currentUnit)
    {
        if (currentUnit.hudSystem.CurCaptureInfo != null)
        {
            petView.SetCaptureAnim(currentUnit.hudSystem.CurCaptureInfo.CaptureAnimType);
        }
    }

    private void SetDamage(Unit currentUnit)
    {
        if (currentUnit.hudSystem.CurDamageInfo != null)
        {
            damageView.SetDamageObject(currentUnit.hudSystem.CurDamageInfo);
        }
    }

    private void SetHeal(Unit currentUnit)
    {
        if (currentUnit.hudSystem.CurHealInfo != null)
        {
            damageView.SetHealObject(currentUnit.hudSystem.CurHealInfo);
        }
    }

    private void SetOtherSidePet(Unit currentUnit)
    {
        if (currentUnit.hudSystem.CurOtherSidePetReactionInfo != null)
        {
            otherSidePetView.SetPetReactionAnim(currentUnit.hudSystem.CurOtherSidePetReactionInfo.ReactionAnimType);
        }
    }
}