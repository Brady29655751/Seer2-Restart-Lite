using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorkshopPetController : Module
{
    [SerializeField] private WorkshopPetModel petModel;
    [SerializeField] private WorkshopPetView petView;
    [SerializeField] private PageView pageView;
    [SerializeField] private WorkshopLearnSkillController learnSkillController;

    protected override void Awake() {
        petModel.onUploadSpriteEvent += OnUploadSpriteSuccess;
    }

    public override void Init() {
        OnPetSetPage();
    }
    
    private void OnPetSetPage() {
        petView.SetPage(petModel.page);
        pageView.SetPage(petModel.page, petModel.lastPage);
    }
    
    public void OnPetPrevPage() {
        petModel.PrevPage();
        OnPetSetPage();
    }

    public void OnPetNextPage() {
        petModel.NextPage();
        OnPetSetPage();
    }

    public void OpenHelpPanel(string type) {
        petView.OpenHelpPanel(type);
    }
    
    public void OnAddSkill() {
        learnSkillController.SetDIYSuccessCallback(OnAddSkillSuccess);
        petView.OpenLearnSkillPanel();
    }

    private void OnAddSkillSuccess(LearnSkillInfo info) {
        petModel.OnAddSkill(info);
        petView.OnAddSkill(info);
        Hintbox.OpenHintboxWithContent("技能学习信息添加成功", 16);
    }

    public void OnRemoveSkill() {
        petModel.OnRemoveSkill();
        petView.OnRemoveSkill();
    }

    public void OnUploadSprite(string type) {
        petModel.OnUploadSprite(type);
    }

    private void OnUploadSpriteSuccess(string type, Sprite sprite) {
        petView.OnUploadSprite(type, sprite);
    }

    public void OnClearSprite(string type) {
        petModel.OnClearSprite(type);
        petView.OnClearSprite(type);
    }

    public void OnPreviewPet() {
        if (!VerifyDIYPet(out var error)) {
            Hintbox.OpenHintboxWithContent(error, 16);
            return;
        }

        var originalInfo = Pet.GetPetInfo(petModel.petInfo.id);
        var petDictionaryPanel = Panel.OpenPanel<PetDictionaryPanel>();

        void OnCompletePreviewPet() {
            Database.instance.petInfoDict.Set(petModel.petInfo.id, originalInfo);
            petDictionaryPanel.onCloseEvent -= OnCompletePreviewPet;
        }

        Database.instance.petInfoDict.Set(petModel.petInfo.id, petModel.petInfo);

        petDictionaryPanel.SelectMode(PetDictionaryMode.WorkshopPreview);
        petDictionaryPanel.SetStorage(new List<Pet>() { Pet.GetExamplePet(petModel.petInfo.id) });
        petDictionaryPanel.onCloseEvent += OnCompletePreviewPet;
    }
    
    public void OnDIYPet() {
        if (!VerifyDIYPet(out var error)) {
            Hintbox.OpenHintboxWithContent(error, 16);
            return;
        }
        var hintbox = Hintbox.OpenHintbox();
        hintbox.SetTitle("提示");
        hintbox.SetContent("记得先导出存档并另外保存\n以避免任何存档毁损而无法游戏之情形\n确认存档保存完整后再按下确认以完成DIY", 16, FontOption.Arial);
        hintbox.SetOptionNum(2);
        hintbox.SetOptionCallback(OnConfirmDIYPet);
    }

    private bool VerifyDIYPet(out string error) {
        return petModel.VerifyDIYPet(out error);
    }

    private void OnConfirmDIYPet() {
        var message = "DIY写入" + (petModel.CreateDIYPet() ? "成功" : "失败");
        Hintbox.OpenHintboxWithContent(message, 16);
    }
    
}
