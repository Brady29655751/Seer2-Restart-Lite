using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using FTRuntime;

public class PetDemoView : Module
{
    // [SerializeField] private bool isAnimAsync = false;
    [SerializeField] private bool featurePromptLeft = false;

    [SerializeField] private Camera animationCamera;
    [SerializeField] private InfoPrompt infoPrompt;
    [SerializeField] private Image demoImage;
    [SerializeField] private Text nameText;
    [SerializeField] private GameObject changeNameObject;

    [SerializeField] private IButton featureButton, featureHighStarButton;
    [SerializeField] private Text featureText;

    [SerializeField] private IButton elementButton, subElementButton;

    [SerializeField] private Image genderImage;
    [SerializeField] private Image specialGenderImage;
    
    [SerializeField] private IButton emblemButton;
    
    [SerializeField] private IButton IVButton;

    private Canvas canvas;
    private GameObject currentPetAnim;
    private Pet currentPet;

    protected override void Awake() {
        canvas = GameObject.Find("Canvas").GetComponent<Canvas>();
    }

    public void SetPet(Pet pet, bool animMode = true) {
        gameObject.SetActive(pet != null);
        if (pet == null) {
            SetAnimation(pet, animMode);
            currentPet = pet;
            return;
        }
        
        SetName(pet.name, PetInfo.IsMod(pet.id));
        SetElement(pet.element, pet.subElement);
        SetFeature(pet);
        SetGender(pet.basic.gender);
        SetEmblem(pet.hasEmblem, pet.feature.emblem);
        SetIVRank(pet.talent.IVRank);
        SetAnimation(pet, animMode);

        currentPet = pet;
    }

    public void SetInfoPromptActive(bool active) {
        infoPrompt.SetActive(active);
    }

    public void SetName(string name, bool isMod) {
        nameText.text = name;
        nameText.color = isMod ? ColorHelper.gold : Color.cyan;
    }

    public void ChangeName(bool isActive)
    {
        changeNameObject?.SetActive(isActive);
    }

    public void SetElement(Element element, Element subElement)
    {
        elementButton?.image?.SetElementSprite(element);
        subElementButton?.image?.SetElementSprite(subElement);
        subElementButton?.gameObject.SetActive(subElement != Element.普通);
    }

    public void SetElementInfoPromptContent(Element element) {
        string text = element.GetElementName();
        Vector2 size = new Vector2(30 + 10 * text.Length, 30);
        infoPrompt.SetInfoPromptWithAutoSize(text, TextAnchor.MiddleCenter);
    }

    public void SetFeature(Pet pet) {
        var feature = pet.feature.feature;
        var buff = Buff.GetFeatureBuff(pet);
        var icon = buff?.info?.GetIcon(false);
        var isIconExist = (icon != null) && ((!PetInfo.IsMod(pet.id)) || buff.info.options.ContainsKey("res"));
        
        featureButton?.gameObject.SetActive(!isIconExist);
        featureHighStarButton?.gameObject.SetActive(isIconExist);

        featureText.text = feature.name.Substring(0, Mathf.Min(feature.name.Length, 2));
        featureHighStarButton?.SetSprite(icon);
    }

    public void SetFeatureInfoPromptContent(Feature feature) {
        string text = feature.description;
        infoPrompt.SetInfoPromptWithAutoSize(text, TextAnchor.MiddleLeft);
        if (featurePromptLeft) 
        {
            infoPrompt.SetPositionOffset(new Vector2(-infoPrompt.size.x, 2));
        } 
        else
        {
            infoPrompt.SetPositionOffset(new Vector2(infoPrompt.zeroFixPos.x, -infoPrompt.size.y / 2 - 2));
        }
    } 

    public void SetGender(int gender) {
        genderImage.SetGenderSprite(gender);
        genderImage.gameObject.SetActive(gender != -1);
        specialGenderImage.gameObject.SetActive(gender == 2);
    }

    public void SetEmblem(bool hasEmblem, Emblem emblem) {
        Sprite nullEmblem = Emblem.GetNullEmblemSprite();
        emblemButton.gameObject.SetActive(emblem != null);
        emblemButton.SetSprite(hasEmblem ? (emblem?.GetSprite() ?? nullEmblem) : nullEmblem);
    }

    public void SetEmblemInfoPromptContent(Emblem emblem, int index = 0) {
        if (emblem == null)
            return;

        string text = emblem.description;
        infoPrompt.SetInfoPromptWithAutoSize(text, TextAnchor.MiddleLeft);
    }

    public void SetIVRank(IVRanking ranking) {
        IVButton?.SetSprite(ranking.GetSprite());
    }

    public void SetIVInfoPromptContent(int iv) {
        string text = "个体值︰" + iv.ToString();
        Vector2 size = new Vector2(95, 30);
        infoPrompt.SetInfoPromptWithAutoSize(text, TextAnchor.MiddleCenter);
    }

    public void SetAnimation(Pet pet, bool isAnimMode = true) {
        if (this.currentPetAnim != null)
            DestroyImmediate(this.currentPetAnim);

        if (pet == null) {
            this.currentPetAnim = null;
            demoImage.gameObject.SetActive(false);
            return;
        }

        if (!isAnimMode) 
        {
            demoImage.gameObject.SetActive(true);
            demoImage.SetSprite(pet.ui.battleImage);
            return;
        }

        /*
        if (isAnimAsync) {
            pet.ui.GetBattleAnimAsync(PetAnimationType.Idle, OnPetAnimLoadCompleted);
            return;
        }
        */
        
        OnPetAnimLoadCompleted(pet.ui.GetBattleAnim(PetAnimationType.Idle));
        
        void OnPetAnimLoadCompleted(GameObject anim) 
        {
            if (this.currentPetAnim != null)
                DestroyImmediate(this.currentPetAnim);

            //检测是否有动画
            if ((this.currentPetAnim = anim) != null)
            {
                this.currentPetAnim.SetActive(true);
                demoImage.gameObject.SetActive(false);

                this.currentPetAnim.transform.SetParent(animationCamera.transform);
                this.currentPetAnim.transform.SetAsFirstSibling();

                SwfClipController controller = this.currentPetAnim.GetComponent<SwfClipController>();
                controller.clip.sortingOrder = 1;

                var animPos = this.currentPetAnim.transform.localPosition;
                this.currentPetAnim.transform.localPosition = new Vector3(animPos.x, animPos.y, 1);
                return;
            }

            // 沒有則使用預設的精靈圖片
            // if (isAnimMode)
            this.currentPetAnim?.SetActive(false);
            
            demoImage.gameObject.SetActive(true);
            demoImage.SetSprite(pet.ui.battleImage);
        }
    }

}
