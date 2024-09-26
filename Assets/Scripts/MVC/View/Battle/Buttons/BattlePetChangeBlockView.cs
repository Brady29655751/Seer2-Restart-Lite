using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattlePetChangeBlockView : Module
{
    private BattlePet pet;
    [SerializeField] private float hpBarLength = 65;
    [SerializeField] private IButton button;
    [SerializeField] private Image icon, element, fightingTag;
    [SerializeField] private Text nameText, levelText, currentHpText, maxHpText;
    [SerializeField] private RectTransform hpBarRect;

    public void SetPet(BattlePet pet) {
        if (pet == null) {
            gameObject.SetActive(false);
            return;
        }
        this.pet = pet;
        SetIcon(  pet.ui.icon);
        SetElement(pet.element);
        SetName(pet.name);
        SetLevel(pet.level);
        SetHp(pet.hp, pet.maxHp);
        SetFightingTag(false);
        SetInteractable(!pet.isDead, pet.isDead);
    }    

    private void SetIcon(Sprite sprite) {
        icon.sprite = sprite;
    }

    private void SetElement(Element element) {
        this.element.SetElementSprite(element);
    }

    private void SetName(string name) {
        nameText.text = name;
    }

    private void SetLevel(int level) {
        levelText.text = "Lv " + level.ToString();
    }

    private void SetHp(int hp, int maxHp) {
        float percent = hp * 1f / maxHp;
        currentHpText.text = hp.ToString();
        maxHpText.text = maxHp.ToString();
        hpBarRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, percent * hpBarLength);
    }

    public void SetChosen(bool chosen) {
        bool interactable = (pet == null) ? false : (!chosen && !pet.isDead);
        SetFightingTag(chosen);
        SetInteractable(interactable, !interactable);
    }

    public void SetFightingTag(bool fighting) {
        fightingTag.gameObject.SetActive(fighting);
    }

    public void SetInteractable(bool interactable, bool grayWhenDisabled) {
        button.SetInteractable(interactable, grayWhenDisabled);

        var imageColor = ((!interactable) && grayWhenDisabled) ? Color.gray : Color.white;
        icon.color = imageColor;
        element.color = imageColor;
    }       

}
