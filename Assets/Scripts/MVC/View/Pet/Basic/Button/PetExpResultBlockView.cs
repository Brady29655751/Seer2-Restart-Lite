using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PetExpResultBlockView : Module
{
    [SerializeField] private PetSelectBlockView selectBlockView;
    [SerializeField] private Text getExpText;
    [SerializeField] private Text levelUpExpText;
    [SerializeField] private Text getEVText;

    public void SetPet(Pet pet) {
        selectBlockView.SetPet(pet);
    }

    public void SetGainExpText(uint exp) {
        getExpText?.SetText("+" + exp);
    }

    public void SetLevelUpExpText(uint exp) {
        levelUpExpText?.SetText((exp == 0) ? "--" : exp.ToString());
    }

    public void SetGainEVText(int ev) {
        getEVText?.SetText("+" + ev);
    }

}
