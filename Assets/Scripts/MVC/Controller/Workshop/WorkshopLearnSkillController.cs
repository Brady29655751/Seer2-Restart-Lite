using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorkshopLearnSkillController : Module
{
    [SerializeField] private WorkshopLearnSkillModel skillModel;
    [SerializeField] private WorkshopLearnSkillView skillView;

    private Action<LearnSkillInfo> onDIYSuccessCallback;

    public override void Init() {
        skillView.OnPreviewSkill(skillModel.learnSkillInfo);
    }

    public void SetDIYSuccessCallback(Action<LearnSkillInfo> callback, bool learnLevelActive = true) {
        onDIYSuccessCallback = callback;
        skillView.SetLearnLevelActive(learnLevelActive);
    }

    public void Search() {
        skillModel.Search();
        skillView.SetSelections(skillModel.skillList, Select);
    }

    public void Select(int index) {
        skillModel.Select(index);
        skillView.OnPreviewSkill(skillModel.learnSkillInfo);
    }

    public void SetInfoPromptActive(bool active) {
        skillView.SetInfoPromptActive(active);
    }

    public void ShowSkillInfo() {
        skillView.ShowSkillInfo(skillModel.currentSkill);
    }

    public void OnCancelLearnSkill() {

    }

    public void OnDIYLearnSkill() {
        if (!VerifyDIYLearnSkill(out var error)) {
            Hintbox.OpenHintboxWithContent(error, 16);
            return;
        }
        OnConfirmDIYLearnSkill();
    }

    private bool VerifyDIYLearnSkill(out string error) {
        return skillModel.VerifyDIYLearnSkill(out error);
    }

    private void OnConfirmDIYLearnSkill() {
        onDIYSuccessCallback?.Invoke(skillModel.learnSkillInfo);
    }

}
