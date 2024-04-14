using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapPetCornerView : Module
{
    [SerializeField] private RectTransform rect;
    [SerializeField] private PetSelectBlockView blockView;
    [SerializeField] private ExtendButton extendButton;

    public void SetPet(Pet pet) {
        blockView.SetPet(pet);
    }

    public void OnAfterHeal() {
        Hintbox hintbox = Hintbox.OpenHintbox();
        hintbox.SetOptionNum(1);
        hintbox.SetTitle("提示");
        hintbox.SetContent("你的精灵已完全恢复！", 16, FontOption.Arial);   
    }

    public void Extend() {
        Vector2 anchorPos = rect.anchoredPosition;
        if ((anchorPos.x != 0) && (anchorPos.x != -150))
            return;

        StartCoroutine(ExtendCoroutine());
    }

    private IEnumerator ExtendCoroutine() {
        Vector2 anchorPos = rect.anchoredPosition;
        bool isShrink = (anchorPos.x == 0);
        int targetX = isShrink ? -150 : 0;
        float speed = isShrink ? -1 : 1;
        while (!(rect.anchoredPosition.x - targetX).IsWithin(-2 * speed, 2 * speed)) {
            rect.anchoredPosition = new Vector2(rect.anchoredPosition.x + speed, rect.anchoredPosition.y);
            yield return null;
        }
        rect.anchoredPosition = new Vector2(targetX, rect.anchoredPosition.y);
        extendButton.SetMode(isShrink);
    }

}
