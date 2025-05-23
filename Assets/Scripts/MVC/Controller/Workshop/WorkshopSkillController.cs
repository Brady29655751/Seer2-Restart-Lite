using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorkshopSkillController : Module
{
    [SerializeField] private WorkshopSkillModel skillModel;
    [SerializeField] private WorkshopSkillView skillView;
    [SerializeField] private WorkshopEffectController effectController;

    public void OpenHelpPanel(string type) {
        skillView.OpenHelpPanel(type);
    }

    public void SetSkill(Skill skill) {
        skillModel.SetSkill(skill);
        skillView.SetSkill(skill, OnEditEffect);
    }

    public void OnAddEffect() {
        effectController.SetDIYSuccessCallback(OnAddEffectSuccess);
        effectController.SetEffect(skillModel.effectList.Count, Effect.GetDefaultEffect());
        skillView.OpenEffectPanel();
    }

    private void OnAddEffectSuccess(Effect effect) {
        skillModel.OnAddEffect(effect);
        skillView.OnAddEffect(effect, OnEditEffect);
        Hintbox.OpenHintboxWithContent("效果添加成功", 16);
    }

    public void OnRemoveEffect() {
        skillModel.OnRemoveEffect();
        skillView.OnRemoveEffect();
    }

    public void OnEditEffect(int index) {
        effectController.SetDIYSuccessCallback((editEffect) => OnEditEffectSuccess(index, editEffect));
        effectController.SetEffect(index, skillModel.effectList[index]);
        skillView.OpenEffectPanel();
    }

    private void OnEditEffectSuccess(int index, Effect editEffect) {
        skillModel.OnEditEffect(index, editEffect);
        skillView.OnEditEffect(index, editEffect);
        Hintbox.OpenHintboxWithContent("效果编辑成功", 16);
    }

    public void OnPreviewDescription() {
        Hintbox.OpenHintboxWithContent(skillModel.descriptionPreview, 14);
    }

    public void OnDIYSkill() {
        /*
        if (Application.platform == RuntimePlatform.Android) {
            Hintbox.OpenHintboxWithContent("手机版工坊入口已关闭\n请用电脑制作Mod", 16);
            return;
        }
        */

        if (!VerifyDIYSkill(out var error)) {
            Hintbox.OpenHintboxWithContent(error, 16);
            return;
        }

        if (Skill.GetSkill(skillModel.id, false) != null) {
            var overwriteHintbox = Hintbox.OpenHintbox();
            overwriteHintbox.SetTitle("提示");
            overwriteHintbox.SetContent("检测到已有相同序号技能，是否确定编辑覆盖？", 16, FontOption.Arial);
            overwriteHintbox.SetOptionNum(2);
            overwriteHintbox.SetOptionCallback(OnConfirmGameDataSaved);
            return;
        }

        OnConfirmGameDataSaved();
    }

    public void OnDeleteSkill() {
        /*
        if (Application.platform == RuntimePlatform.Android) {
            Hintbox.OpenHintboxWithContent("手机版工坊入口已关闭\n请用电脑制作Mod", 16);
            return;
        }
        */

        var hintbox = Hintbox.OpenHintbox();
        hintbox.SetTitle("提示");
        hintbox.SetContent("确定要删除此技能吗？\n记得先保存存档！", 16, FontOption.Arial);
        hintbox.SetOptionNum(2);
        hintbox.SetOptionCallback(() => {
            skillModel.DeleteDIYSkill(out string message);
            Hintbox.OpenHintboxWithContent(message, 16);
        });
    }   

    private bool VerifyDIYSkill(out string error) {
        return skillModel.VerifyDIYSkill(out error);
    }

    private void OnConfirmGameDataSaved() {
        var hintbox = Hintbox.OpenHintbox();
        hintbox.SetTitle("提示");
        hintbox.SetContent("记得先导出存档并另外保存\n以避免任何存档毁损而无法游戏之情形\n确认存档保存完整后再按下确认以完成DIY", 16, FontOption.Arial);
        hintbox.SetOptionNum(2);
        hintbox.SetOptionCallback(OnConfirmDIYSkill);
    }

    private void OnConfirmDIYSkill() {
        skillModel.CreateDIYSkill(out var message);
        Hintbox.OpenHintboxWithContent(message, 16);
    }


}
