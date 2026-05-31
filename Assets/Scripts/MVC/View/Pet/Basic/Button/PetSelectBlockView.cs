using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PetSelectBlockView : Module
{
    private Pet currentPet = null;
    private bool isNull => currentPet == null;
    private float initHpBarLength;
    private Vector2 initButtonSize, initIconSize;
    private Sprite overrideFrameSprite = null;

    [SerializeField] private bool interactableWhenNull = false;
    [SerializeField] private bool showGender = false;
    [SerializeField] private bool showIconFrame = true;
    [SerializeField] private IButton button;
    [SerializeField] private Sprite chosenFrameSprite;
    [SerializeField] private Sprite notChosenFrameSprite;
    [SerializeField] private Text nameText;
    [SerializeField] private Image mask, icon, frame, getMark;
    [SerializeField] private Text levelText;
    [SerializeField] private Text hpText;
    [SerializeField] private RectTransform hpBarRect;
    [SerializeField] private List<GameObject> genderObjects;

    protected override void Awake()
    {
        base.Awake();
        initHpBarLength = (hpBarRect == null) ? 0 : hpBarRect.rect.size.x;
        InitButtonAndIconSize();
    }

    public void InitButtonAndIconSize()
    {
        initButtonSize = initButtonSize == Vector2.zero ? (button?.rect?.rect.size ?? Vector2.zero) : initButtonSize;
        initIconSize = initIconSize == Vector2.zero ? (icon?.rectTransform?.rect.size ?? Vector2.zero) : initIconSize;
    }

    public void SetPet(Pet p)
    {
        currentPet = p;
        button.SetInteractable(interactableWhenNull || (!isNull));
    
        SetFrame();
        SetMask();
        SetFrameSprite();
        SetIcon();

        SetLevel();
        SetName();
        SetHp();
        SetGender();
    }

    public void SetGetMark(bool isGet)
    {
        getMark?.gameObject.SetActive(isGet);
    }

    public void SetInteractable(bool interactable)
    {
        button.SetInteractable(interactable);
    }

    public void SetChosen(bool chosen)
    {
        if (SetFrameSprite())
            return;
        
        button.SetSprite(chosen ? chosenFrameSprite : notChosenFrameSprite);
    }

    public void SetFrame()
    {
        if (!showIconFrame)
        {
            overrideFrameSprite = null;
            button.SetSize(initButtonSize);
            return;
        }

        var itemId = currentPet?.record?.GetRecord("iconFrame", 0) ?? 0;
        var buttonSizeOffset = currentPet?.record?.GetRecord("iconFrameSizeOffset", "0,0")?.ToVector2() ?? Vector2.zero;

        overrideFrameSprite = Item.GetItemInfo(itemId)?.icon;
        button.SetSize(initButtonSize + buttonSizeOffset / 90 * initButtonSize);
    }

    public bool SetFrameSprite()
    {
        var isOverride = (overrideFrameSprite != null) && (mask != null);
        if (isOverride)
            button.SetSprite(overrideFrameSprite);

        button.SetColor(isOverride ? Color.clear : Color.white);
        frame.SetSprite(overrideFrameSprite ?? SpriteSet.Empty);
        return isOverride;
    }

    public void SetMask()
    {
        if (mask == null)
            return;

        var record = currentPet?.record;
        if ((record == null) || (!showIconFrame))
        {
            mask.SetSprite(null);
            icon.SetSize(initIconSize);
            return;
        }
            
        var frameId = record.GetRecord("iconFrame", 0);
        var maskId = record.GetRecord("iconMask", frameId);
        var iconSizeOffset = record.GetRecord("iconSizeOffset", "0,0")?.ToVector2() ?? Vector2.zero;
        var res = Item.GetItemInfo(maskId)?.icon;

        mask.SetSprite(res);
        icon.SetSize(initIconSize + iconSizeOffset / 80 * initIconSize);
    }

    private void SetIcon()
    {
        // icon.gameObject.SetActive(true);
        icon.SetSprite(isNull ? notChosenFrameSprite : currentPet.ui.icon);
    }

    private void SetLevel()
    {
        if (levelText != null)
        {
            levelText.text = isNull ? string.Empty : currentPet.level.ToString();
        }
    }

    private void SetName()
    {
        if (nameText != null)
        {
            nameText.text = isNull ? "未知" : currentPet.name;
        }
    }

    private void SetHp()
    {
        if ((hpText == null) || (hpBarRect == null))
            return;

        int currentHp = (int)(isNull ? 0 : currentPet.currentStatus.hp);
        int normalHp = (int)(isNull ? 0 : currentPet.normalStatus.hp);
        hpText.text = isNull ? string.Empty : (currentHp.ToString() + " / " + normalHp.ToString());
        hpBarRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, isNull ? 0 : (initHpBarLength * currentHp / normalHp));
    }

    public void SetHpBarActive(bool active)
    {
        hpBarRect?.gameObject.SetActive(active);
    }

    public void SetGender()
    {
        if (ListHelper.IsNullOrEmpty(genderObjects))
            return;

        for (int i = 0; i < genderObjects.Count; i++)
        {
            genderObjects[i].SetActive(showGender && (!isNull) && (i == currentPet.basic.gender));
        }
    }

    public void SetGenderIconActive(bool active)
    {
        showGender = active;
        SetGender();
    }

}
