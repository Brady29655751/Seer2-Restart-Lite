using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorkshopEffectController : Module
{
    [SerializeField] private WorkshopEffectModel effectModel;
    [SerializeField] private WorkshopEffectView effectView;
    [SerializeField] private WorkshopLearnSkillController learnSkillController;
    [SerializeField] private WorkshopLearnBuffController learnBuffController;

    private Action<Effect> onDIYSuccessCallback;

    private List<Effect> multiEffectList = new List<Effect>();

    public void OpenHelpPanel(string type) {
        effectView.OpenHelpPanel(type);
    }

    public void OnCancelEffect() {
       
    }

    public void OnAbilityChanged(int abilityDropdownValue) {
        // effectModel.OnAbilityChanged();
        // effectView.SetAbility(effectModel.ability, effectModel.abilityDetail);
    }

    public void OnReferSkill() {
        learnSkillController.SetDIYSuccessCallback(OnReferSkillSuccess, false);
        effectView.SetRefSkillPanelActive(true);
    }

    private void OnReferSkillSuccess(LearnSkillInfo skillInfo) {   
        multiEffectList = skillInfo.skill.effects;

        if (multiEffectList.Count > 1) {
            effectView.SetMultiEffectPanelActive(true);
            effectView.SetMultiEffectList(multiEffectList, OnSelectReferredEffect);
            return;
        }

        OnSelectReferredEffect(0);
    }

    public void OnReferBuff() {
        learnBuffController.SetDIYSuccessCallback(OnReferBuffSuccess);
        effectView.SetRefBuffPanelActive(true);
    }

    private void OnReferBuffSuccess(BuffInfo buffInfo) {
        multiEffectList = buffInfo.effects;

        if (multiEffectList.Count > 1) {
            effectView.SetMultiEffectPanelActive(true);
            effectView.SetMultiEffectList(multiEffectList, OnSelectReferredEffect);
            return;
        }

        OnSelectReferredEffect(0);
    }

    public void OnSelectReferredEffect(int index) {
        SetEffect(index, multiEffectList[index]);
        Hintbox.OpenHintboxWithContent("效果参考成功", 16);
    }

    public void SetEffect(int index, Effect effect) {
        effectModel.SetReferredEffect(index, effect);
        effectView.SetRefSkillPanelActive(false);
        effectView.SetRefBuffPanelActive(false);
        effectView.SetMultiEffectPanelActive(false);
    }

    public void SetDIYSuccessCallback(Action<Effect> callback) {
        onDIYSuccessCallback = callback;
    }

    public void OnDIYEffect() {
        if (!VerifyDIYEffect(out var error)) {
            Hintbox.OpenHintboxWithContent(error, 16);
            return;
        }
        OnConfirmDIYEffect();
    }

    private bool VerifyDIYEffect(out string error) {
        return effectModel.VerifyDIYEffect(out error);
    }

    private void OnConfirmDIYEffect() {
        effectView.SetEffectPanelActive(false);
        onDIYSuccessCallback?.Invoke(effectModel.effect);
    }


}
