using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PetSwitchView : Module
{
    [SerializeField] private Text nameText;
    
    public void SetPet(Pet pet) {
        SetName(pet?.name);
    }

    public void SetName(string name) {
        nameText?.SetText(name);
    }

    public void OnConfirmSwitch() {
        Hintbox hintbox = Hintbox.OpenHintbox();
        hintbox.SetTitle("提示");
        hintbox.SetContent("切换成功！\n快去设定学习力、技能、性格吧！", 14, FontOption.Arial);
        hintbox.SetOptionNum(1);
    }
}
