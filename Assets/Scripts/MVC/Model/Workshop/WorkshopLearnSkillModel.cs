using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WorkshopLearnSkillModel : Module
{
    public const int MAX_SEARCH_COUNT = 50;
    [SerializeField] private IInputField searchInputField, idInputField, learnLevelInputField;

    public int id => (int.TryParse(idInputField.inputString, out var skillId)) ? skillId : 0;
    public int learnLevel => (int.TryParse(learnLevelInputField.inputString, out var level)) ? level : 1;

    public List<Skill> skillList = new List<Skill>();
    public Skill currentSkill => Skill.GetSkill(id, false);
    public LearnSkillInfo learnSkillInfo => GetLearnSkillInfo();

    public LearnSkillInfo GetLearnSkillInfo() {
        return new LearnSkillInfo(currentSkill, learnLevel);
    }

    public void Search() {
        if (string.IsNullOrEmpty(searchInputField.inputString)) {
            Hintbox.OpenHintboxWithContent("搜索名称不能为空！", 16);
            return;
        }

        idInputField.SetInputString(string.Empty);

        // Try search skills with same name.
        var skillDict = Database.instance.skillDict;
        skillList = skillDict.Where(x => x.Value.name == searchInputField.inputString)
            .OrderBy(x => x.Key).Select(x => x.Value).Take(MAX_SEARCH_COUNT).ToList();

        if (skillList.Count > 0)
            return;

        // No skills with same name. Search similar result.
        skillList = skillDict.Where(x => x.Value.name.Contains(searchInputField.inputString))
            .OrderBy(x => x.Key).Select(x => x.Value).Take(MAX_SEARCH_COUNT).ToList();
    }

    public void Select(int index) {
        if (!index.IsInRange(0, skillList.Count)) {
            idInputField.SetInputString(string.Empty);
            return;
        }

        idInputField.SetInputString(skillList[index]?.id.ToString() ?? string.Empty);

        if (!int.TryParse(learnLevelInputField.inputString, out _))
            learnLevelInputField.SetInputString("1");
    }

    public bool VerifyDIYLearnSkill(out string error) {
        error = string.Empty;

        if (string.IsNullOrEmpty(idInputField.inputString) || string.IsNullOrEmpty(learnLevelInputField.inputString)) {
            error = "技能序号与学习等级不能为空！";
            return false;
        }

        if (Skill.GetSkill(id, false) == null) {
            error = "该技能序号不存在！";
            return false;
        }

        if ((!int.TryParse(learnLevelInputField.inputString, out _)) || (!learnLevel.IsWithin(1, 100))) {
            error = "学习等级必须介于1到100之间";
            return false;
        }

        return true;
    }
}
