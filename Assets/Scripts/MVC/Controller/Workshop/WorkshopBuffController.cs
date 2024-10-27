using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorkshopBuffController : Module
{
    [SerializeField] private WorkshopBuffModel buffModel;
    [SerializeField] private WorkshopBuffView buffView;
    [SerializeField] private WorkshopEffectController effectController;

    protected override void Awake() {
        buffModel.onUploadIconEvent += OnUploadIconSuccess;
    }

    public void OpenHelpPanel(string type) {
        buffView.OpenHelpPanel(type);
    }

    public void SetBuffInfo(BuffInfo buffInfo) {
        buffModel.SetBuffInfo(buffInfo);
        buffView.SetBuffInfo(buffInfo, OnEditEffect);
    }

    public void OnUploadIcon() {
        buffModel.OnUploadIcon();
    }

    private void OnUploadIconSuccess(Sprite sprite) {
        buffView.OnUploadIcon(sprite);
    }

    public void OnClearIcon() {
        buffModel.OnClearIcon();
        buffView.OnClearIcon();
    }

    public void OnAddEffect() {
        effectController.SetDIYSuccessCallback(OnAddEffectSuccess);
        effectController.SetEffect(buffModel.effectList.Count, Effect.GetDefaultEffect());
        buffView.OpenEffectPanel();
    }

    private void OnAddEffectSuccess(Effect effect) {
        buffModel.OnAddEffect(effect);
        buffView.OnAddEffect(effect, OnEditEffect);

        Hintbox.OpenHintboxWithContent("效果添加成功", 16);
    }

    public void OnRemoveEffect() {
        buffModel.OnRemoveEffect();
        buffView.OnRemoveEffect();
    }

    public void OnEditEffect(int index) {
        effectController.SetDIYSuccessCallback((editEffect) => OnEditEffectSuccess(index, editEffect));
        effectController.SetEffect(index, buffModel.effectList[index]);
        buffView.OpenEffectPanel();
    }

    private void OnEditEffectSuccess(int index, Effect editEffect) {
        buffModel.OnEditEffect(index, editEffect);
        buffView.OnEditEffect(index, editEffect);
        Hintbox.OpenHintboxWithContent("效果编辑成功", 16);
    }

    public void OnPreviewDescription() {
        Hintbox.OpenHintboxWithContent(buffModel.descriptionPreview, 14);
    }

    public void OnDIYBuff() {
        /*
        if (Application.platform == RuntimePlatform.Android) {
            Hintbox.OpenHintboxWithContent("手机版工坊入口已关闭\n请用电脑制作Mod", 16);
            return;
        }
        */

        if (!VerifyDIYBuff(out var error)) {
            Hintbox.OpenHintboxWithContent(error, 16);
            return;
        }

        if (Buff.GetBuffInfo(buffModel.id) != null) {
            var overwriteHintbox = Hintbox.OpenHintbox();
            overwriteHintbox.SetTitle("提示");
            overwriteHintbox.SetContent("检测到已有相同序号印记，是否确定编辑覆盖？", 16, FontOption.Arial);
            overwriteHintbox.SetOptionNum(2);
            overwriteHintbox.SetOptionCallback(OnConfirmGameDataSaved);
            return;
        }

        OnConfirmGameDataSaved();
    }

    public void OnDeleteBuff() {
        /*
        if (Application.platform == RuntimePlatform.Android) {
            Hintbox.OpenHintboxWithContent("手机版工坊入口已关闭\n请用电脑制作Mod", 16);
            return;
        }
        */

        var hintbox = Hintbox.OpenHintbox();
        hintbox.SetTitle("提示");
        hintbox.SetContent("确定要删除此印记吗？\n记得先保存存档！", 16, FontOption.Arial);
        hintbox.SetOptionNum(2);
        hintbox.SetOptionCallback(() => {
            buffModel.DeleteDIYBuff(out string message);
            Hintbox.OpenHintboxWithContent(message, 16);
        });
    }   

    private bool VerifyDIYBuff(out string error) {
        return buffModel.VerifyDIYBuff(out error);
    }

    private void OnConfirmGameDataSaved() {
        var hintbox = Hintbox.OpenHintbox();
        hintbox.SetTitle("提示");
        hintbox.SetContent("记得先导出存档并另外保存\n以避免任何存档毁损而无法游戏之情形\n确认存档保存完整后再按下确认以完成DIY", 16, FontOption.Arial);
        hintbox.SetOptionNum(2);
        hintbox.SetOptionCallback(OnConfirmDIYBuff);
    }

    private void OnConfirmDIYBuff() {
        buffModel.CreateDIYBuff(out var message);
        Hintbox.OpenHintboxWithContent(message, 16);
    }
}
