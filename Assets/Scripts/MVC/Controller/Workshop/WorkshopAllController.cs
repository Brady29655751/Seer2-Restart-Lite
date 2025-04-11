using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorkshopAllController : Module
{
    [SerializeField] private WorkshopAllModel allModel;
    [SerializeField] private WorkshopAllView allView;
    [SerializeField] private OptionSelectController optionSelectController;
    [SerializeField] private WorkshopPetController petController;
    [SerializeField] private WorkshopSkillController skillController;
    [SerializeField] private WorkshopBuffController buffController;
    [SerializeField] private WorkshopItemController itemController;

    private PetDictionaryPanel petDictionaryPanel = null;

    public override void Init() {
        if (SaveSystem.IsModExists())
            allView.CheckCurrentMod();
        else
            allView.NeverCreateMod();
    }

    public void CreateMod() {
        if (!SaveSystem.TryCreateMod(out var error)) {
            Hintbox.OpenHintboxWithContent(error, 16);
            return;
        }
        Hintbox.OpenHintboxWithContent("创建成功，开始发挥你的创意吧！", 16);
        CheckCurrentMod();
    }

    private void CheckCurrentMod() {
        allView.CheckCurrentMod();
    }

    public void OpenModPetDictionaryPanel() {
        try {
            var petStorage = Database.instance.petInfoDict.Where(entry => PetInfo.IsMod(entry.Key))
                .OrderByDescending(entry => entry.Key).Select(entry => Pet.GetExamplePet(entry.Key)).ToList();

            if (petStorage.Count == 0) {
                Hintbox.OpenHintboxWithContent("你目前还没有制作任何创意精灵哦！", 16);
                return;
            }

            petDictionaryPanel = Panel.OpenPanel<PetDictionaryPanel>();
            petDictionaryPanel.SetEditPetCallback(OnEditPet);
            petDictionaryPanel.SelectMode(PetDictionaryMode.Workshop);
            petDictionaryPanel.SetStorage(petStorage);
        } catch (Exception e) {
            var hintbox = Hintbox.OpenHintboxWithContent("打开创意精灵图鉴发生错误，错误如下：\n" + e.ToString(), 14);
            hintbox.SetSize(600, 400);
        }
    }

    public void OpenAllSkillPanel() {
        allView.SetAllSkillPanelActive(true);
    }

    public void OpenAllBuffPanel() {
        allView.SetAllBuffPanelActive(true);
    }

    public void OpenAllItemPanel() {
        allView.SetAllItemPanelActive(true);
    }

    public void OnEditPet(PetInfo petInfo) {
        optionSelectController.Select(1);
        petController.SetPetInfo(petInfo);
    }

    public void OnEditSkill() {
        if (allModel.currentSkill == null) 
            return;

        if (allModel.currentSkill.IsAction()) {
            Hintbox.OpenHintboxWithContent("无法查看该技能", 16);
            return;
        }

        optionSelectController.Select(2);
        skillController.SetSkill(allModel.currentSkill);
    }

    public void OnEditSkill(Skill skill) {
        if (skill == null)
            return;

        optionSelectController.Select(2);
        skillController.SetSkill(skill);
        allView.SetAllSkillPanelActive(false);
    }

    public void OnEditBuff() {
        if (allModel.currentBuffInfo == null)
            return;

        optionSelectController.Select(3);
        buffController.SetBuffInfo(allModel.currentBuffInfo);
    }

    public void OnEditItem() {
        if (allModel.currentItemInfo == null)
            return;

        optionSelectController.Select(4);
        itemController.SetItemInfo(allModel.currentItemInfo);
    }

    public void OnEditItem(ItemInfo itemInfo) {
        if (itemInfo == null)
            return;

        optionSelectController.Select(4);
        itemController.SetItemInfo(itemInfo);
        allView.SetAllItemPanelActive(false);
    }
    
    public void OnImportMod() {
        allModel.OnImportMod();
    }
    
    public void OnExportMod() {
        allModel.OnExportMod();
    }

    public void OnUpdateMod() {
        allModel.OnUpdateMod();
    }

    public void OnDeleteMod() {
        allModel.OnDeleteMod();
    }

}
