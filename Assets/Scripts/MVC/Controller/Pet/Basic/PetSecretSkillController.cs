using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PetSecretSkillController : Module
{
    [SerializeField] private PetSecretSkillModel secretSkillModel;
    [SerializeField] private PetSecretSkillView secretSkillView;
    [SerializeField] private PageView pageView;
    
    public void SetPet(Pet pet) {
        secretSkillModel.SetPet(pet);
        OnSetPage();
    }

    public void SetInfoPromptActive(bool active) {
        secretSkillView.SetInfoPromptActive(active);
    }

    public void SetSecretSkillInfo(int index) {
        if (!index.IsInRange(0, secretSkillModel.selections.Length)) {
            SetInfoPromptActive(false);
            return;
        }

        secretSkillView.SetSkillInfoPromptContent(secretSkillModel.selections[index]?.skill);
    }

    public void OnSetPage()
    {
        secretSkillView.SetSecretSkillInfo(secretSkillModel.selections);
        pageView.SetPage(secretSkillModel.page, secretSkillModel.lastPage);
    }

    public void PrevPage()
    {
        secretSkillModel.PrevPage();
        OnSetPage();
    }

    public void NextPage()
    {
        secretSkillModel.NextPage();
        OnSetPage();
    }

    public void OnBuySecretSkill() {
        var skills = secretSkillModel.currentPet.skills;
        if (skills.secretSkillInfo.Where(x => x.secretType != SecretType.Others)
            .Select(x => x.skill.id).All(skills.ownSkillId.Contains)) {
            Hintbox.OpenHintboxWithContent("该精灵已习得所有一般隐藏技能", 16);
            return;
        }

        var hintbox = Hintbox.OpenHintbox();
        hintbox.SetTitle("提示");
        hintbox.SetContent("确定要花费<color=#ffbb33>50星钻</color>一键习得所有一般隐藏技能吗？\n（特殊隐藏技能无法一键学习）", 14, FontOption.Arial);
        hintbox.SetOptionCallback(OnConfirmBuySecretSkill);
    }

    private void OnConfirmBuySecretSkill() {
        Item currency = Item.Find(Item.DIAMOND_ID);
        if ((currency == null) || (currency.num < 50)) {
            Hintbox.OpenHintboxWithContent("星钻不足无法购买", 16);
            return;
        }
        var skills = secretSkillModel.currentPet.skills;
        var secretSkill = skills.secretSkillInfo.Where(x => x.secretType != SecretType.Others).Select(x => x.skill);
        foreach (var skill in secretSkill)
            secretSkillModel.currentPet.skills.LearnNewSkill(skill);

        Item.Remove(Item.DIAMOND_ID, 50);
        Hintbox.OpenHintboxWithContent("一键学习成功\n请重新开启精灵背包查看", 14);
    }

}
