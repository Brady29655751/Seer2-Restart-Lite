using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PetStatusController : Module
{
    [SerializeField] private PetStatusModel statusModel; 
    [SerializeField] private PetStatusView statusView;
    [SerializeField] private PetEVView evView;  // may absent

    public event Action<Pet> onSetEVSuccessEvent;

    protected override void Awake()
    {
        base.Awake();
    }

    public override void Init()
    {
        base.Init();
    }

    public void SetPet(Pet pet) {
        statusModel.SetPet(pet);
        statusView.SetPet(statusModel.currentPet);
        evView?.SetPet(statusModel.currentPet);

        // Clean max ev mode.
        if (statusModel.isMaxEVMode) {
            statusModel.isMaxEVMode = false;
            statusView.SetMaxEVBoxActive(false);
        }
    }

    public void OnResetEVButtonClick() {
        if (statusModel.isSettingEV) {
            statusModel.OnAfterSetEV(false);
            statusView.OnAfterSetEV(statusModel.status, statusModel.ev);
            evView?.OnAfterSetEV(statusModel.evStorage);
        } else {
            if (Item.Find(10237) == null) {
                Hintbox hintbox = Hintbox.OpenHintbox();
                hintbox.SetTitle("提示");
                hintbox.SetContent("未持有学习力重置道具", 14, FontOption.Arial);
                hintbox.SetOptionNum(1);
                return;
            }

            if (statusModel.ev.sum <= 0)
                return;
            statusModel.OnBeforeResetEV();
            statusView.OnBeforeSetEV(statusModel.status, statusModel.ev);
            evView?.OnBeforeSetEV(statusModel.evStorage);
        }
    }

    public void OnSetEVButtonClick() {
        if (statusModel.isSettingEV) 
        {
            statusModel.OnAfterSetEV(true);
            statusView.OnAfterSetEV(statusModel.status, statusModel.ev);
            evView?.OnAfterSetEV(statusModel.evStorage);

            if (statusModel.isMaxEVMode) {
                statusModel.isMaxEVMode = false;
                statusView.SetMaxEVBoxActive(false);
            }

            onSetEVSuccessEvent?.Invoke(statusModel.currentPet);
        } 
        else 
        {
            if (statusModel.evStorage <= 0)
                return;

            statusModel.OnBeforeSetEV();
            statusView.OnBeforeSetEV(statusModel.status, statusModel.ev);
            evView?.OnBeforeSetEV(statusModel.evStorage);

            if (statusModel.isMaxEVMode)
                statusView.SetMaxEVBoxActive(true);
        }
    }

    public void OnMaxEVButtonClick() {
        if (statusModel.evStorage <= 0)
            return;

        statusModel.isMaxEVMode = true;
        OnSetEVButtonClick();
    }

    private void SetEVWithSpeed(int type, int speed) {
        bool sign = Mathf.Sign(speed) > 0;
        int add = Mathf.Abs(speed);
        for (int i = 0; i < add; i++) {
            statusModel.OnSetEV(type, sign);
            statusView.OnSetEV(statusModel.status, statusModel.ev);
            evView?.OnSetEV(statusModel.evStorage);
        }
    }

    public void OnAddEV(int type) {
        SetEVWithSpeed(type, statusModel.evSpeed);

        if (statusModel.isMaxEVMode)
            OnSetEVButtonClick();
    }

    public void OnMinusEV(int type) {
        SetEVWithSpeed(type, -statusModel.evSpeed);

        if (statusModel.isMaxEVMode)
            OnSetEVButtonClick();
    }

    public void OnAddEVButtonHold(int type) {
        if (statusModel.isMaxEVMode)
            return;

        SetEVWithSpeed(type, 5 * statusModel.evSpeed);
    }

    public void OnMinusEVButtonHold(int type) {
        if (statusModel.isMaxEVMode)
            return;

        SetEVWithSpeed(type, -5 * statusModel.evSpeed);
    }

    [Obsolete]
    public void OnMaxEV(int type) {
        statusModel.OnMaxEV(type);
        statusView.OnSetEV(statusModel.status, statusModel.ev);
        statusView.SetMaxEVBoxActive(false);
        evView?.OnSetEV(statusModel.evStorage);
        onSetEVSuccessEvent?.Invoke(statusModel.currentPet);
    }    
}
