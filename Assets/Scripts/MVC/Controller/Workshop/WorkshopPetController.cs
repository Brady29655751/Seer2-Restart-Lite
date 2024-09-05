using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorkshopPetController : Module
{
    [SerializeField] private WorkshopPetModel petModel;
    [SerializeField] private WorkshopPetView petView;
    [SerializeField] private PageView pageView;
    [SerializeField] private WorkshopLearnSkillController learnSkillController;
    [SerializeField] private WorkshopLearnBuffController learnBuffController;

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

    public void SetPetInfo(PetInfo petInfo) {
        petModel.SetPage(0);
        OnPetSetPage();

        petModel.SetPetInfo(petInfo);
        petView.SetPetInfo(petInfo);
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

    public void OnSelectFeature() {
        learnBuffController.SetDIYSuccessCallback(OnSelectFeatureSuccess);
        petView.OpenLearnBuffPanel();
    }

    private void OnSelectFeatureSuccess(BuffInfo info) {
        petModel.OnSelectFeature(info);
        Hintbox.OpenHintboxWithContent("特性自动填写成功", 16);
    }    

    public void OnSelectEmblem() {
        learnBuffController.SetDIYSuccessCallback(OnSelectEmblemSuccess);
        petView.OpenLearnBuffPanel();
    }

    private void OnSelectEmblemSuccess(BuffInfo info) {
        petModel.OnSelectEmblem(info);
        Hintbox.OpenHintboxWithContent("纹章自动填写成功", 16);
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
        petView.OnPreviewPet();
    }
    
    public void OnDIYPet() {
        if (!VerifyDIYPet(out var error)) {
            Hintbox.OpenHintboxWithContent(error, 16);
            return;
        }

        if (Pet.GetPetInfo(petModel.petInfo.id) != null) {
            var overwriteHintbox = Hintbox.OpenHintbox();
            overwriteHintbox.SetTitle("提示");
            overwriteHintbox.SetContent("检测到已有相同序号精灵，是否确定编辑覆盖？\n注意：编辑精灵会失去目前获得的同序号精灵", 16, FontOption.Arial);
            overwriteHintbox.SetOptionNum(2);
            overwriteHintbox.SetOptionCallback(OnConfirmGameDataSaved);
            return;
        }

        OnConfirmGameDataSaved();
    }
    
    public void OnDeletePet() {
        var hintbox = Hintbox.OpenHintbox();
        hintbox.SetTitle("提示");
        hintbox.SetContent("确定要删除此精灵吗？\n记得先保存存档！", 16, FontOption.Arial);
        hintbox.SetOptionNum(2);
        hintbox.SetOptionCallback(() => {
            petModel.DeleteDIYPet(out string message);
            Hintbox.OpenHintboxWithContent(message, 16);
        });
    }   

    private bool VerifyDIYPet(out string error) {
        return petModel.VerifyDIYPet(out error);
    }

    private void OnConfirmGameDataSaved() {
        var hintbox = Hintbox.OpenHintbox();
        hintbox.SetTitle("提示");
        hintbox.SetContent("记得先导出存档并另外保存\n以避免任何存档毁损而无法游戏之情形\n确认存档保存完整后再按下确认以完成DIY", 16, FontOption.Arial);
        hintbox.SetOptionNum(2);
        hintbox.SetOptionCallback(OnConfirmDIYPet);
    }

    private void OnConfirmDIYPet() {
        var message = "DIY写入" + (petModel.CreateDIYPet() ? "成功" : "失败");
        Hintbox.OpenHintboxWithContent(message, 16);
    }
    
}
