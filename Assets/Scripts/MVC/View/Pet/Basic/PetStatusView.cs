using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PetStatusView : Module
{
    [SerializeField] private GameObject normalEVBox, maxEVBox;
    [SerializeField] private IButton maxEVButton;
    [SerializeField] private PetStatusBlockView[] statusBlockViews = new PetStatusBlockView[6];

    private Status extraStatus = Status.zero;

    public void SetPet(Pet pet) {
        gameObject.SetActive(pet != null);
        if (pet == null) {
            SetExtraStatus(Status.zero);
            return;
        }

        SetExtraStatus(pet.extraStatus);
        SetStatus(pet.normalStatus);
        SetEV(pet.talent.ev);
        SetEVButtonsActive(false);
    }

    public void OnBeforeSetEV(Status status, Status ev) {
        SetStatus(status);
        SetEV(ev);
        SetEVButtonsActive(true);
    }

    public void OnSetEV(Status status, Status ev) {
        SetStatus(status);
        SetEV(ev);
    }

    public void OnAfterSetEV(Status status, Status ev) {
        SetStatus(status);
        SetEV(ev);
        SetEVButtonsActive(false);
    }

    private void SetStatus(Status status) {
        for (int i = 0; i < 6; i++) {
            statusBlockViews[i].SetStatus(status[i] + extraStatus[i], i);
        }
    }

    private void SetExtraStatus(Status extraStatus) {
        this.extraStatus = new Status(extraStatus);

        for (int i = 0; i < 6; i++) {
            var statusColor = (extraStatus[i] == 0) ? ColorHelper.normalSkill : ColorHelper.gold;
            statusBlockViews[i].SetStatusColor(statusColor);
        }
    }

    private void SetEV(Status ev) {
        for (int i = 0; i < 6; i++) {
            statusBlockViews[i].SetEV(ev[i]);
        }
    }

    private void SetEVButtonsActive(bool active) {
        for (int i = 0; i < 6; i++) {
            statusBlockViews[i].SetEVButtonsActive(active, true);
            statusBlockViews[i].SetEVButtonsActive(active, false);
        }
        maxEVButton?.gameObject.SetActive(!active);
    }

    public void SetMaxEVBoxActive(bool active) {
        normalEVBox?.SetActive(!active);
        maxEVBox?.SetActive(active);
    }
}
