using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using FTRuntime;

public class PetSkinView : Module
{
    [SerializeField] private List<PetSelectBlockView> petSelectBlockViews;
    [SerializeField] private Image skinImage;
    private GameObject currentPetAnim;

    public void SetSkins(List<int> skinIds) {
        for (int i = 0; i < petSelectBlockViews.Count; i++) {
            Pet pet = Pet.GetExamplePet((i < skinIds.Count) ? skinIds[i] : 0);
            petSelectBlockViews[i].SetPet(pet);
        }
    }

    public void Select(int index) {
        for (int i = 0; i < petSelectBlockViews.Count; i++) {
            petSelectBlockViews[i].SetChosen(i == index);
        }        
    }

    public void SetPetAnimation(int skinId) {
        var uiInfo = Pet.GetPetInfo(skinId)?.ui;
        // 發現會和背包裡的demoView搶動畫
        /*  
        if (this.currentPetAnim != null)
            DestroyImmediate(this.currentPetAnim);

        if (uiInfo == null) {
            this.currentPetAnim = null;
            skinImage.gameObject.SetActive(false);
            return;
        }

        //检测是否有动画
        if ((this.currentPetAnim = uiInfo.GetBattleAnim(PetAnimationType.Idle)) != null) 
        {
            this.currentPetAnim.SetActive(true);
            skinImage.gameObject.SetActive(false);

            this.currentPetAnim.transform.SetParent(transform);
            this.currentPetAnim.transform.SetAsFirstSibling();
            this.currentPetAnim.transform.localScale *= 0.6f;
            SwfClipController controller = this.currentPetAnim.GetComponent<SwfClipController>();
            controller.clip.sortingOrder = 1;
            this.currentPetAnim.transform.position = new Vector3(this.currentPetAnim.transform.position.x * 0.54f + 4.5f,
                this.currentPetAnim.transform.position.y * 0.6f + 0.2f, 0);
            return;
        }
        */
        // 沒有則使用預設的精靈圖片
        this.currentPetAnim?.SetActive(false);
        skinImage.gameObject.SetActive(true);
        skinImage.SetSprite(uiInfo?.battleImage);
    }

    public void OnSkinConfirm() {
        // Hintbox hintbox = Hintbox.OpenHintbox();
        // hintbox.SetTitle("提示");
        // hintbox.SetContent("成功替换精灵皮肤", 16, FontOption.Arial);
        // hintbox.SetOptionNum(1);
    }
}
