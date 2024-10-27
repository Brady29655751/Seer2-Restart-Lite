using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorkshopItemController : Module
{
    [SerializeField] private WorkshopItemModel itemModel;
    [SerializeField] private WorkshopItemView itemView;
    [SerializeField] private WorkshopEffectController effectController;

    protected override void Awake() {
        itemModel.onUploadIconEvent += OnUploadIconSuccess;
    }

    public void OpenHelpPanel(string type) {
        itemView.OpenHelpPanel(type);
    }

    public void SetItemInfo(ItemInfo itemInfo) {
        itemModel.SetItemInfo(itemInfo);
        itemView.SetItemInfo(itemInfo, OnEditEffect);
    }

    public void OnUploadIcon() {
        itemModel.OnUploadIcon();
    }

    private void OnUploadIconSuccess(Sprite sprite) {
        itemView.OnUploadIcon(sprite);
    }

    public void OnClearIcon() {
        itemModel.OnClearIcon();
        itemView.OnClearIcon();
    }

    public void OnAddEffect() {
        effectController.SetDIYSuccessCallback(OnAddEffectSuccess);
        effectController.SetEffect(itemModel.effectList.Count, Effect.GetDefaultEffect());
        itemView.OpenEffectPanel();
    }

    private void OnAddEffectSuccess(Effect effect) {
        itemModel.OnAddEffect(effect);
        itemView.OnAddEffect(effect, OnEditEffect);

        Hintbox.OpenHintboxWithContent("效果添加成功", 16);
    }

    public void OnRemoveEffect() {
        itemModel.OnRemoveEffect();
        itemView.OnRemoveEffect();
    }

    public void OnEditEffect(int index) {
        effectController.SetDIYSuccessCallback((editEffect) => OnEditEffectSuccess(index, editEffect));
        effectController.SetEffect(index, itemModel.effectList[index]);
        itemView.OpenEffectPanel();
    }

    private void OnEditEffectSuccess(int index, Effect editEffect) {
        itemModel.OnEditEffect(index, editEffect);
        itemView.OnEditEffect(index, editEffect);
        Hintbox.OpenHintboxWithContent("效果编辑成功", 16);
    }

    public void OnDIYItem() {
        /*
        if (Application.platform == RuntimePlatform.Android) {
            Hintbox.OpenHintboxWithContent("手机版工坊入口已关闭\n请用电脑制作Mod", 16);
            return;
        }
        */

        if (!VerifyDIYItem(out var error)) {
            Hintbox.OpenHintboxWithContent(error, 16);
            return;
        }

        if (Item.GetItemInfo(itemModel.id) != null) {
            var overwriteHintbox = Hintbox.OpenHintbox();
            overwriteHintbox.SetTitle("提示");
            overwriteHintbox.SetContent("检测到已有相同序号道具，是否确定编辑覆盖？", 16, FontOption.Arial);
            overwriteHintbox.SetOptionNum(2);
            overwriteHintbox.SetOptionCallback(OnConfirmGameDataSaved);
            return;
        }

        OnConfirmGameDataSaved();
    }

    public void OnDeleteItem() {
        /*
        if (Application.platform == RuntimePlatform.Android) {
            Hintbox.OpenHintboxWithContent("手机版工坊入口已关闭\n请用电脑制作Mod", 16);
            return;
        }
        */

        var hintbox = Hintbox.OpenHintbox();
        hintbox.SetTitle("提示");
        hintbox.SetContent("确定要删除此道具吗？\n记得先保存存档！", 16, FontOption.Arial);
        hintbox.SetOptionNum(2);
        hintbox.SetOptionCallback(() => {
            itemModel.DeleteDIYItem(out string message);
            Hintbox.OpenHintboxWithContent(message, 16);
        });
    }   

    private bool VerifyDIYItem(out string error) {
        return itemModel.VerifyDIYItem(out error);
    }

    private void OnConfirmGameDataSaved() {
        var hintbox = Hintbox.OpenHintbox();
        hintbox.SetTitle("提示");
        hintbox.SetContent("记得先导出存档并另外保存\n以避免任何存档毁损而无法游戏之情形\n确认存档保存完整后再按下确认以完成DIY", 16, FontOption.Arial);
        hintbox.SetOptionNum(2);
        hintbox.SetOptionCallback(OnConfirmDIYItem);
    }

    private void OnConfirmDIYItem() {
        itemModel.CreateDIYItem(out var message);
        Hintbox.OpenHintboxWithContent(message, 16);
    }
}
