using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PetResistController : Module
{
    [SerializeField] private PetResistModel resistModel;
    [SerializeField] private PetResistView resistView;
    [SerializeField] private GameObject learnBuffPanel;
    [SerializeField] private WorkshopLearnBuffController learnBuffController;

    public event Action<Pet> onSetEVSuccessEvent;

    public void SetPet(Pet pet)
    {
        resistModel.SetPet(pet);
        resistView.SetPetResist(resistModel.resist);
        resistView.SetEVButtonsActive(false);
    }

    public void UnlockPetResist()
    {
        resistModel.UnlockPetResist();
        resistView.SetPetResist(resistModel.resist);
    }

    public void ShowBuffResistInfo(string type)
    {
        if (resistModel.resist == null)
            return;

        switch (type)
        {
            default:
                return;

            case "abnormal":
                resistView.ShowBuffInfo(resistModel.tmpResist.abnormalBuff);
                return;

            case "unhealthy":
                resistView.ShowBuffInfo(resistModel.tmpResist.unhealthyBuff);
                return;
        }        
    }

    public void OnSetBuffButtonClick(string type)
    {
        learnBuffPanel.SetActive(true);
        learnBuffController.SetStorage(BuffInfo.database.Where(x => x.type == type.ToBuffType()).ToList());
        learnBuffController.SetDIYSuccessCallback(buffInfo => SetBuffResistType($"{type}.id", buffInfo?.id ?? 0));
    }

    public void OnSetResistButtonClick()
    {
        resistModel.OnAfterSetEV(true);
        resistView.SetPetResist(resistModel.resist);
        resistView.SetEVButtonsActive(false);

        onSetEVSuccessEvent?.Invoke(resistModel.currentPet);
    }

    public void OnResetResistButtonClick()
    {
        resistModel.OnBeforeSetEV(false);
        resistView.SetPetResist(resistModel.resist);
        resistView.SetEVButtonsActive(true);
    }

    public void OnMaxResistButtonClick()
    {
        resistModel.OnBeforeSetEV(true);
        resistView.SetPetResist(resistModel.resist);
        resistView.SetEVButtonsActive(true);
    }

    private void SetEVWithSpeed(string type, int speed) {
        bool sign = speed >= 0;
        int add = Mathf.Abs(speed);

        resistModel.evSpeed = add;
        resistModel.OnSetEV(type, sign);
        resistView.SetPetResist(resistModel.tmpResist);
    }

    public void OnAddEV(string type) {
        SetEVWithSpeed(type, resistModel.isMaxEVMode ? 255 : 1);
    }

    public void OnMinusEV(string type) {
        SetEVWithSpeed(type, resistModel.isMaxEVMode ? -255 : -1);
    }

    public void OnAddEVButtonHold(string type) {
        SetEVWithSpeed(type, 5);
    }

    public void OnMinusEVButtonHold(string type) {
        SetEVWithSpeed(type, -5);
    }

    private void SetBuffResistType(string type, int value)
    {
        if (Buff.GetBuffInfo(value) == null)
        {
            Hintbox.OpenHintboxWithContent($"无法设置该印记为为状态抗性", 16);
            return;
        }

        resistModel.OnSetBuffResistType(type, value);

        if (!resistModel.isSettingEV)
        {
            OnSetResistButtonClick();
            return;
        }

        resistView.SetPetResist(resistModel.tmpResist);
    }
}
