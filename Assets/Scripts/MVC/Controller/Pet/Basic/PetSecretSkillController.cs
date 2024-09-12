using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PetSecretSkillController : Module
{
    [SerializeField] private PetSecretSkillModel secretSkillModel;
    [SerializeField] private PetSecretSkillView secretSkillView;

    public void SetPet(Pet pet) {
        secretSkillModel.SetPet(pet);
        secretSkillView.SetSecretSkillInfo(secretSkillModel.secretSkillInfos);
    }

    public void SetInfoPromptActive(bool active) {
        secretSkillView.SetInfoPromptActive(active);
    }

    public void SetSecretSkillInfo(int index) {
        if (!index.IsInRange(0, secretSkillModel.secretSkillInfos.Length)) {
            SetInfoPromptActive(false);
            return;
        }
        secretSkillView.SetSkillInfoPromptContent(secretSkillModel.secretSkillInfos[index].skill);
    }

    public void OnBuySecretSkill() {
        var skills = secretSkillModel.currentPet.skills;
        if (skills.secretSkillId.All(skills.ownSkillId.Contains)) {
            Hintbox.OpenHintboxWithContent("该精灵已习得所有隐藏技能", 16);
            return;
        }

        var hintbox = Hintbox.OpenHintbox();
        hintbox.SetTitle("提示");
        hintbox.SetContent("确定要花费<color=#ffbb33>50星钻</color>一键习得所有隐藏技能吗？", 14, FontOption.Arial);
        hintbox.SetOptionCallback(OnConfirmBuySecretSkill);
    }

    private void OnConfirmBuySecretSkill() {
        Item currency = Item.Find(Item.DIAMOND_ID);
        if ((currency == null) || (currency.num < 50)) {
            Hintbox.OpenHintboxWithContent("星钻不足无法购买", 16);
            return;
        }
        foreach (var skill in secretSkillModel.currentPet.skills.secretSkill)
            secretSkillModel.currentPet.skills.LearnNewSkill(skill);

        Item.Remove(Item.DIAMOND_ID, 50);
        Hintbox.OpenHintboxWithContent("一键学习成功\n请重新开启精灵背包查看", 14);
    }

}
