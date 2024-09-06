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
    }

    public void OpenAllSkillPanel() {
        allView.OpenAllSkillPanel();
    }

    public void OpenAllBuffPanel() {
        allView.OpenAllBuffPanel();
    }

    public void OpenAllItemPanel() {
        allView.OpenAllItemPanel();
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
