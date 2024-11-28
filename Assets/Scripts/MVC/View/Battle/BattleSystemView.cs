using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;

public class BattleSystemView : BattleBaseView
{
    [SerializeField] private Timer timer;
    [SerializeField] private BattleResourceView resourceView;
    [SerializeField] private BattleOptionView optionView;
    [SerializeField] private BattleWeatherView weatherView;
    [SerializeField] private BattlePetBuffView stateBuffView;
    [SerializeField] private BattleAudioView audioView;
    [SerializeField] private BattleResultView resultView;

    protected override void Awake()
    {
        base.Awake();
        timer.gameObject.SetActive(battle.settings.mode == BattleMode.PVP);
        timer.onStartEvent += OnTimerStart;
        timer.onDoneEvent += OnTimerDone;
    }

    public void StartTimer() {
        timer.SetTimer(battle.settings.time);
    }

    public void DoneTimer() {
        if (timer.isDone)
            return;
        
        timer.Done();
    }

    public void StopTimer() {
        timer.secondText?.SetText(string.Empty);
    }

    private void OnTimerStart() {
        timer.secondText?.SetSize(72);
    }

    private void OnTimerDone(float leftTime) {
        timer.secondText?.SetSize(48);
        timer.secondText?.SetText("等待对手出招");

        Skill defaultSkill = Skill.GetNoOpSkill();
        Unit myUnit = ((battle.currentPhase?.phase == EffectTiming.OnPassivePetChange) ? battle.currentPhase.state : battle.currentState).myUnit;

        // If run out of time then set default skill
        if (leftTime == 0) {
            if (myUnit.pet.isDead) {
                int newCursor = myUnit.petSystem.petBag.FindIndex(x => !x.isDead);
                defaultSkill = Skill.GetPetChangeSkill(myUnit.petSystem.cursor, newCursor, true);
            } else {
                defaultSkill = myUnit.pet.skillController.GetDefaultSkill(myUnit.pet.anger);
            }
            battle.SetSkill(defaultSkill, true);
        }
    }

    public void ProcessResult(BattleResult result) {
        resultView.ProcessResult(result);
    }

    public bool ShowPetSecretSkillResult() {
        bool isShowingSecretSkill = battle.result.learnedSecretSkills.Count > 0;
        if (isShowingSecretSkill)
            resultView.ShowPetSecretSkillResult();
        return isShowingSecretSkill;
    }

    public void SetBottomBarInteractable(bool interactable) {
        optionView.SetBottomBarInteractable(interactable);
        if (interactable)
            StartTimer();
        else   
            DoneTimer();
    }

    public void SelectOption(int index) {
        optionView.Select(index);
    }

    public void SetOptionActive(int index, bool active) {
        optionView.SetOptionActive(index, active);
    }

    public void SetSkillSelectMode(bool isSkillSelectMode) {
        optionView.SetSkillSelectMode(isSkillSelectMode);
    }

    public void SetState(BattleState lastState, BattleState currentState) {
        if (currentState == null)
            return;

        var stateBuffs = currentState.stateBuffs.Select(x => x.Value).ToList();
        if (currentState.weather != 0)
            stateBuffs.Insert(0, currentState.weatherBuff);

        optionView.SetState(lastState, currentState);
        weatherView.SetState(lastState, currentState);
        stateBuffView.SetBuff(stateBuffs);
    }

}
