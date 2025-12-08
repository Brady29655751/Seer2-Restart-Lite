using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PetSkillCardView : Module
{
    public Skill currentSkill { get; private set; }
    private bool isNull => currentSkill == null;

    public RectTransform rectTransform;
    [SerializeField] private IDraggable dragController;
    [SerializeField] private IButton skillButton;
    [SerializeField] private Image artwork;
    [SerializeField] private Image angerTensDigit, angerUnitsDigit;
    [SerializeField] private IText nameText, effectText;

    public void SetSkill(Skill skill, BattleRule rule = BattleRule.Anger)
    {
        currentSkill = skill;
        skillButton.SetInteractable(!isNull, false);
        SetName();
        SetElement();
        SetAnger(rule);
        SetEffect();
        SetArtwork();
    }

    public void SetArtwork()
    {
        if (isNull)
        {
            artwork.sprite = SpriteSet.Empty;
            return;
        }

        var petId = (int)currentSkill.GetSkillIdentifier("option[belongPet.skinId]");
        var sprite = Pet.GetPetInfo(petId)?.ui.idleImage;
        artwork.SetSprite(sprite ?? SpriteSet.Empty);

        if (sprite != null)
            artwork.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, sprite.GetResizedWidth(artwork.rectTransform.rect.size.y));
    }

    private void SetName()
    {
        nameText.SetText(isNull ? string.Empty : currentSkill.name);
    }

    private void SetElement()
    {
        skillButton.SetSprite(SpriteSet.GetCardFrameSprite(isNull ? Element.普通 : currentSkill.element));
        nameText.SetColor(isNull ? Color.white : PetElementSystem.GetElementColor(currentSkill.element));
    }

    private void SetAnger(BattleRule rule = BattleRule.Anger)
    {
        var anger = isNull ? -1 : rule switch
        {
            BattleRule.Anger => currentSkill.anger,
            BattleRule.PP => currentSkill.PP,
            _ => -1,
        };

        if (anger < 0)
        {
            angerTensDigit.SetSprite(ResourceManager.instance.GetSprite($"Numbers/Blue/%"));
            angerUnitsDigit.SetSprite(ResourceManager.instance.GetSprite($"Numbers/Blue/%"));
            return;
        }

        anger = Mathf.Clamp(anger, 0, 99);
        angerTensDigit.SetSprite(ResourceManager.instance.GetSprite($"Numbers/Blue/{anger / 10}"));
        angerUnitsDigit.SetSprite(ResourceManager.instance.GetSprite($"Numbers/Blue/{anger % 10}"));
    }

    private void SetEffect()
    {
        var desc = currentSkill?.GetAdditionalCardDescription(currentSkill.rawDescription, false);
        effectText.SetText(isNull ? string.Empty : Skill.GetSkillDescriptionPreview(desc));
    }

    public void SetDraggable(bool draggable)
    {
       dragController?.SetEnable(draggable);
    }

    public void SetRaycastTarget(bool raycastTarget)
    {
        if (skillButton?.image == null)
            return;

        skillButton.image.raycastTarget = raycastTarget;
    }

    public void SetCallback(Action callback, string type)
    {
        skillButton.SetCallback(callback, type);
    }
}
