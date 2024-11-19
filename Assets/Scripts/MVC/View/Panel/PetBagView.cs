using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PetBagView : Module
{
    [SerializeField] private IButton firstButton, trainButton;
    [SerializeField] private GameObject itemPanelObject;

    public void OnSelect(int index) {
        firstButton.SetInteractable(index != 0);
    }

    public void OnSetPetHeal() {
        Hintbox hintbox = Hintbox.OpenHintbox();
        hintbox.SetOptionNum(1);
        hintbox.SetTitle("提示");
        hintbox.SetContent("你的精灵已完全恢复！", 16, FontOption.Arial);
    }

    public void OnSetPetTrain() {
        Hintbox hintbox = Hintbox.OpenHintbox();
        hintbox.SetOptionNum(1);
        hintbox.SetTitle("提示");
        hintbox.SetContent("锻炼成功！\n请自行分配学习力", 16, FontOption.Arial);
    }

    public void OnSetPetHomeFailed() {
        Hintbox hintbox = Hintbox.OpenHintbox();
        hintbox.SetOptionNum(1);
        hintbox.SetTitle("不可入库");
        hintbox.SetContent("你至少需要一只精灵保护自己", 16, FontOption.Arial);
    }

    public void TogglePetItemPanel() {
        if (itemPanelObject == null)
            return;

        itemPanelObject.SetActive(!itemPanelObject.activeSelf);
    }
}
