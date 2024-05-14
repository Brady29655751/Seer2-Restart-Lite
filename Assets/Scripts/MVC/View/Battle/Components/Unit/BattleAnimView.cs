using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleAnimView : BattleBaseView
{
    // Note that the damage view is on enemy's side.
    [SerializeField] private BattlePetAnimView petView;
    [SerializeField] private BattleSkillBubbleAnimView skillBubbleView;
    [SerializeField] private BattleDamageAnimView damageView;

    public bool isDone => petView.isDone;

    public void SetUnit(Unit lastUnit, Unit currentUnit) {
        if (currentUnit == null)
            return;

        SetPet(lastUnit, currentUnit);
        SetSkillBubble(lastUnit, currentUnit);
        SetDamage(lastUnit, currentUnit);
    }

    private void SetPet(Unit lastUnit, Unit currentUnit) {
        // int lastCursor = (lastUnit == null) ? -1 : lastUnit.petSystem.cursor;
        var lastPetId = lastUnit?.pet?.id ?? 0;
        var currentPetId = currentUnit?.pet?.id ?? 0;
        if (lastPetId != currentPetId) {
            petView.SetPet(currentUnit.pet);
            return;
        }
        if (currentUnit.hudSystem.applyPetAnim) {
            petView.SetPetAnim(currentUnit.skillSystem.skill, currentUnit.hudSystem.petAnimType);
        }
    }

    private void SetSkillBubble(Unit lastUnit, Unit currentUnit) {
        bool currentBubble = currentUnit.hudSystem.applySkillBubbleAnim;
        bool lastBubble = (lastUnit == null) ? (!currentBubble) : lastUnit.hudSystem.applySkillBubbleAnim;

        if (lastBubble == currentBubble)
            return;

        if (!currentUnit.hudSystem.applySkillBubbleAnim) {
            skillBubbleView.SetActive(false);
            return;
        }
        skillBubbleView.SetSkill(currentUnit.skill);
        skillBubbleView.SetActive(true);
    }

    private void SetDamage(Unit lastUnit, Unit currentUnit) {
        if (!currentUnit.hudSystem.applyDamageSystemAnim)
            return;

        damageView.SetUnit(lastUnit, currentUnit);
    }
}
