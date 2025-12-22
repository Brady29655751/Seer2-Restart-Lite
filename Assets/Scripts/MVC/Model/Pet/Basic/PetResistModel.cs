using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PetResistModel : Module
{
    public Pet currentPet { get; private set; }
    public PetResist resist => currentPet?.resist;

    public bool isSettingEV { get; private set; }
    public bool isMaxEVMode { get; private set; }
    public int evSpeed = 1;
    public Dictionary<string, int> resistDict = new Dictionary<string, int>();
    public PetResist tmpResist => (resistDict == null) ? null : new PetResist(resistDict){ lockState = resist?.lockState ?? -1 };
    

    public void SetPet(Pet pet)
    {
        currentPet = pet;
        resistDict = resist?.ToDictionary();
        isSettingEV = false;
        evSpeed = 1;
    }   

    public void UnlockPetResist()
    {
        if (currentPet?.resist == null)
            return;

        currentPet.resist.lockState = 1;
    }

    public void OnBeforeSetEV(bool maxEVMode) {
        if (currentPet == null) 
            return;

        isSettingEV = true;
        isMaxEVMode = maxEVMode;
    }

    public void OnSetEV(string type, bool positive = true) {
        if (!isSettingEV)
            return;

        int sign = positive ? 1 : -1;
        var max = ((type ==  "abnormal.value") || (type == "unhealthy.value")) ? 55 : 35;

        resistDict.Set(type, Mathf.Clamp(resistDict.Get(type, 0) + sign * evSpeed, 0, max));
    }

    public void OnAfterSetEV(bool success) {
        if (currentPet == null)
            return;

        if (success) 
        {
            currentPet.resist.SetResistDict(resistDict);
            SaveSystem.SaveData();
        }

        resistDict = resist?.ToDictionary();
        isSettingEV = false;
    }

    public void OnSetBuffResistType(string type, int value)
    {
        resistDict.Set(type, value);
    }
}
