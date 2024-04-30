using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorkshopEffectController : Module
{
    [SerializeField] private WorkshopEffectModel effectModel;
    [SerializeField] private WorkshopEffectView effectView;

    private Action<Effect> onDIYSuccessCallback;

    public void OpenHelpPanel(string type) {
        effectView.OpenHelpPanel(type);
    }

    public void OnCancelEffect() {
       
    }

    public void OnAbilityChanged(int abilityDropdownValue) {
        effectModel.OnAbilityChanged();
        effectView.SetAbility(effectModel.ability, effectModel.abilityDetail);
    }

    public void OnAddConditionOptions() {
        
    }

    public void OnRemoveConditionOptions() {
        effectModel.OnRemoveConditionOptions();
        effectView.OnRemoveConditionOptions();
    }

    public void OnAddAbilityOptions() {

    }

    public void OnRemoveAbilityOptions() {
        effectModel.OnRemoveAbilityOptions();
        effectView.OnRemoveAbilityOptions();
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
