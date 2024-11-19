using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

public class PetStatusModel : Module
{
    public Pet currentPet;
    public bool isSettingEV { get; private set; }
    public bool isMaxEVMode {
        get => evSpeed == 255;
        set => evSpeed = value ? 255 : 1;
    }
    public int evSpeed { get; private set; } = 1;
    public int evStorage => tmpEVStorage;
    public Status ev => new Status(tmpEV);
    public Status status => tmpStatus;
    
    private int tmpEVStorage;
    private Status tmpEV = new Status();
    private Status tmpStatus { 
        get => Status.GetPetNormalStatus(currentPet.level, currentPet.info.basic.baseStatus,
        currentPet.talent.iv, tmpEV, currentPet.basic.personality); 
    }

    public void SetPet(Pet pet) {
        currentPet = pet;
        isSettingEV = false;
        evSpeed = 1;
        if (currentPet == null)
            return;
        tmpEV = new Status(currentPet.talent.ev);
        tmpEVStorage = currentPet.talent.evStorage;
    }

    public void OnBeforeResetEV() {
        if (currentPet == null)
            return;
        
        if (currentPet.talent.ev.sum <= 0)
            return;

        tmpEV = Status.zero;
        tmpEVStorage = (int)currentPet.talent.ev.sum + currentPet.talent.evStorage;
        isSettingEV = true;
    }

    public void OnBeforeSetEV() {
        if (currentPet == null) 
            return;

        if (currentPet.talent.evStorage <= 0)
            return;
        
        tmpEV = new Status(currentPet.talent.ev);
        tmpEVStorage = currentPet.talent.evStorage;
        isSettingEV = true;
    }

    public void OnSetEV(int type, bool positive = true) {
        if (!isSettingEV)
            return;

        int add = positive ? 1 : -1;
        if (!((int)tmpEV[type] + add).IsWithin(0, 255))
            return;

        if (!((int)tmpEV.sum + add).IsWithin(0, 510))
            return;

        if (!(tmpEVStorage - add).IsWithin(0, 510))
            return;

        tmpEV[type] += add;
        tmpEVStorage -= add;
    }

    public void OnAfterSetEV(bool success) {
        if (currentPet == null)
            return;
        
        if (success) {
            currentPet.talent.SetEV(tmpEV);
            currentPet.talent.SetEVStorage(tmpEVStorage);
            currentPet.currentStatus = Status.Min(currentPet.currentStatus, currentPet.normalStatus);
            SaveSystem.SaveData();
        } else {
            tmpEV = new Status(currentPet.talent.ev);
            tmpEVStorage = currentPet.talent.evStorage;
        }
        isSettingEV = false;
    }

    [Obsolete]
    public void OnMaxEV(int type) {
        int storage = currentPet.talent.evStorage;
        int ev = (int)currentPet.talent.ev[type];
        int add = Mathf.Clamp(255 - ev, 0, storage);
        if (add <= 0)
            return;
    
        tmpEV = new Status(currentPet.talent.ev);
        tmpEV[type] = ev + add;
        tmpEVStorage = storage - add;
        OnAfterSetEV(true);
    }

}
