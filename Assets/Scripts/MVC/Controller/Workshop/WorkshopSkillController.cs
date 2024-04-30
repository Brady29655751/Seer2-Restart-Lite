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

    public void OnAddEffect() {
        effectController.SetDIYSuccessCallback(OnAddEffectSuccess);
        skillView.OpenEffectPanel();
    }

    private void OnAddEffectSuccess(Effect effect) {
        skillModel.OnAddEffect(effect);
        skillView.OnAddEffect(effect);
        Hintbox.OpenHintboxWithContent("效果添加成功", 16);
    }

    public void OnRemoveEffect() {
        skillModel.OnRemoveEffect();
        skillView.OnRemoveEffect();
    }

    public void OnPreviewDescription() {
        Hintbox.OpenHintboxWithContent(skillModel.descriptionPreview, 18);
    }

    public void OnDIYSkill() {
        if (!VerifyDIYSkill(out var error)) {
            Hintbox.OpenHintboxWithContent(error, 16);
            return;
        }
        var hintbox = Hintbox.OpenHintbox();
        hintbox.SetTitle("提示");
        hintbox.SetContent("记得先导出存档并另外保存\n以避免任何存档毁损而无法游戏之情形\n确认存档保存完整后再按下确认以完成DIY", 16, FontOption.Arial);
        hintbox.SetOptionNum(2);
        hintbox.SetOptionCallback(OnConfirmDIYSkill);
    }   

    private bool VerifyDIYSkill(out string error) {
        return skillModel.VerifyDIYSkill(out error);
    }

    private void OnConfirmDIYSkill() {
        var message = "DIY写入" + (skillModel.CreateDIYSkill() ? "成功" : "失败");
        Hintbox.OpenHintboxWithContent(message, 16);
    }


}
