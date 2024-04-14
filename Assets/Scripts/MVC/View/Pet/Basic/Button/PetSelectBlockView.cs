using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PetSelectBlockView : Module
{
    private Pet currentPet = null;
    private bool isNull => (currentPet == null);
    private float initHpBarLength;
    [SerializeField] private IButton button;
    [SerializeField] private Sprite chosenFrameSprite;
    [SerializeField] private Sprite notChosenFrameSprite;
    [SerializeField] private Text nameText;
    [SerializeField] private Image icon;
    [SerializeField] private Text levelText;
    [SerializeField] private Text hpText;
    [SerializeField] private RectTransform hpBarRect;

    protected override void Awake()
    {
        base.Awake();
        initHpBarLength = (hpBarRect == null) ? 0 : hpBarRect.rect.size.x;
    }

    public void SetPet(Pet p) {
        currentPet = p;
        button.SetInteractable(!isNull);
        SetIcon();
        SetLevel();
        SetName();
        SetHp();
    }

    public void SetChosen(bool chosen) {
        button.SetSprite(chosen ? chosenFrameSprite : notChosenFrameSprite);
    }

    private void SetIcon() {
        icon.gameObject.SetActive(!isNull);
        icon.sprite = isNull ? null : currentPet.ui.icon;
    }

    private void SetLevel() {
        if (levelText != null) {
            levelText.text = isNull ? string.Empty : currentPet.level.ToString();
        }
    }

    private void SetName() {
        if (nameText != null) {
            nameText.text = isNull ? "未知" : currentPet.name;
        }
    }

    private void SetHp() {
        if ((hpText != null) && (hpBarRect != null)) {
            int currentHp = (int)(isNull ? 0 : currentPet.currentStatus.hp);
            int normalHp = (int)(isNull ? 0 : currentPet.normalStatus.hp);
            hpText.text = isNull ? string.Empty : (currentHp.ToString() + " / " + normalHp.ToString());
            hpBarRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, isNull ? 0 : (initHpBarLength * currentHp / normalHp));
        }
    }   

}
