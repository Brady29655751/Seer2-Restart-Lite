using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WorkshopSkillModel : Module
{
    [SerializeField] private IInputField idInputField, nameInputField;
    [SerializeField] private IDropdown elementDropdown, typeDropdown;
    [SerializeField] private IInputField powerInputField, angerInputField, accuracyInputField, 
        priorityInputField, descriptionInputField, optionInputField;

    public Skill skill => GetSkill();
    public int id => int.Parse(idInputField.inputString);
    public string skillName => nameInputField.inputString;
    public Element element => (Element)(elementDropdown.value);
    public SkillType type => GetSkillTypeFromDropdown();
    public int power => int.Parse(powerInputField.inputString);
    public string cost => angerInputField.inputString;
    public int accuracy => int.Parse(accuracyInputField.inputString);
    public int priority => string.IsNullOrEmpty(priorityInputField.inputString) ? 0 : int.Parse(priorityInputField.inputString);
    
    public string description => descriptionInputField.inputString?.Replace("\n", "[ENDL]") ?? string.Empty;
    public string descriptionPreview => Skill.GetSkillDescriptionPreview(description);
    public string options => optionInputField.inputString;
    public string optionsAll => ((priority == 0) ? string.Empty : ("priority=" + priority)) + (string.IsNullOrEmpty(options) ? string.Empty : ("&" + options)).TrimEnd("&");

    public List<Effect> effectList = new List<Effect>();

    protected override void Awake()
    {
        // if (!PetElementSystem.IsMod())
        //     return;

        elementDropdown.SetDropdownOptions(PetElementSystem.elementNameList);
    }
    
    public SkillType GetSkillTypeFromDropdown()
    {
        if (typeDropdown.value < 4)
            return (SkillType)typeDropdown.value;
        else if (typeDropdown.value == 4)
            return SkillType.必杀;
        else
            return (SkillType)(typeDropdown.value - 10);
    }

    public Skill GetSkill() {
        var skill = new Skill(id, skillName, element, type, power, cost, accuracy, optionsAll, description);
        skill.SetEffects(effectList.Select(x => new Effect(x)).ToList());
        return skill;
    }

    public void SetSkill(Skill skill) {
        idInputField.SetInputString(skill.id.ToString());
        nameInputField.SetInputString(skill.name);

        elementDropdown.value = (int)skill.element;
        typeDropdown.value = (skill.type < 0) ? ((int)skill.type + 10) : Mathf.Min((int)skill.type, 4);

        powerInputField.SetInputString(skill.power.ToString());
        angerInputField.SetInputString(skill.rawCostString);
        accuracyInputField.SetInputString(skill.accuracy.ToString());
        priorityInputField.SetInputString(skill.priority.ToString());

        descriptionInputField.SetInputString(skill.rawDescription.Replace("[ENDL]", "\n"));

        var rawOptions = new Dictionary<string, string>(skill.options);
        rawOptions.Remove("priority");
        optionInputField.SetInputString(rawOptions.Select(entry => entry.Key + "=" + entry.Value).ConcatToString("&"));

        effectList = skill.effects.Select(x => new Effect(x)).ToList();
    }

    public void OnAddEffect(Effect effect) {
        effectList.Add(effect);
    }

    public void OnRemoveEffect() {
        if (effectList.Count == 0)
            return;

        effectList.RemoveAt(effectList.Count - 1);
    }

    public void OnEditEffect(int index, Effect effect) {
        if (!index.IsInRange(0, effectList.Count))
            return;

        effectList[index] = effect;
    }

    public bool CreateDIYSkill(out string message) {
        var originalSkill = Skill.GetSkill(skill.id, false);
        Database.instance.skillDict.Set(skill.id, skill);
        if (SaveSystem.TrySaveSkillMod(skill, out var error)) {
            message = "DIY写入成功";
            return true;
        }

        // rollback
        Database.instance.skillDict.Set(skill.id, originalSkill);
        message = "DIY写入失败\n" + error;
        return false;
    }

    public bool DeleteDIYSkill(out string message) {
        if (!VerifyId(out message))
            return false;

        var originalSkill = Skill.GetSkill(id, false);
        if ((originalSkill == null) || (!Skill.IsMod(id))) {
            message = "未检测到此序号的Mod技能";
            return false;
        }

        Database.instance.skillDict.Remove(id);
        if (SaveSystem.TrySaveSkillMod(originalSkill, out var error, id)) {
            message = "技能删除成功";
            return true;
        }

        // rollback
        Database.instance.skillDict.Set(id, originalSkill);
        message = "技能删除失败\n" + error;
        return false;
    }

    public bool VerifyDIYSkill(out string error) {
        error = string.Empty;

        if (!VerifyId(out error))
            return false;

        if (!VerifyName(out error))
            return false;

        if (!VerifyPowerAngerAccuracyPriority(out error))
            return false;

        if (!VerifyOptions(out error))
            return false;        

        if (!VerifyDescription(out error))
            return false;

        return true;
    }

    private bool VerifyId(out string error) {
        error = string.Empty;

        if (string.IsNullOrEmpty(idInputField.inputString)) {
            error = "序号不能为空！";
            return false;
        }

        if (!int.TryParse(idInputField.inputString, out _)) {
            error = "序号需为整数！";
            return false;
        }

        if (id > -10001) {
            error = "序号需小于等于-10001\n请点击序号右方的问号查看说明";
            return false;
        }

        return true;
    }

    private bool VerifyName(out string error) {
        error = string.Empty;

        if (string.IsNullOrEmpty(nameInputField.inputString)) {
            error = "名字不能为空！";
            return false;
        }

        if (nameInputField.inputString.Contains(',')) {
            error = "名字不能有半形逗号";
            return false;
        }

        return true;
    }

    private bool VerifyPowerAngerAccuracyPriority(out string error) {
        error = string.Empty;

        if (string.IsNullOrEmpty(powerInputField.inputString) || 
            string.IsNullOrEmpty(angerInputField.inputString) || 
            string.IsNullOrEmpty(accuracyInputField.inputString)) {
            error = "威力、怒气、命中不能为空！";
            return false;
        }

        var costList = cost.ToIntList('/');
        if (ListHelper.IsNullOrEmpty(costList))
        {
            error = "怒气格式错误！";
            return false;
        }

        if ((power < 0) || costList.Exists(x => x < 0) || (accuracy < 0)) {
            error = "威力、怒气、命中不能为负数！";
            return false;
        }

        if (!int.TryParse(priorityInputField.inputString, out _)) {
            error = "先制需为整数！";
            return false;
        }

        return true;
    }

    private bool VerifyOptions(out string error) {
        error = string.Empty;

        if (optionsAll.Contains(',')) {
            error = "【其他】部分不能有半形逗号";
            return false;
        }

        var dict = new Dictionary<string, string>();
        try {
            dict.ParseOptions(optionsAll);
        } catch (Exception) {
            error = "【其他】部分有重复或残缺的自定义选项";
            return false;
        }

        if ((!int.TryParse(dict.Get("critical", "5"), out var critical)) || (critical < 0)) 
        {
            error = "【其他】自定义选项的【暴击率】\n必须为0以上的数字或不填写";
            return false;
        }

        if (!bool.TryParse(dict.Get("ignore_shield", "false"), out var ignoreShield)) {
            error = "【其他】自定义选项的【无视护盾】\n必须为true或false或不填写";
            return false;
        }

        if (!bool.TryParse(dict.Get("ignore_powerup", "false"), out var ignorePowerup)) {
            error = "【其他】自定义选项的【无视能力变化】\n必须为true或false或不填写";
            return false;
        }

        return true;
    }

    private bool VerifyDescription(out string error) {  
        error = string.Empty;

        if (string.IsNullOrEmpty(descriptionInputField.inputString)) {
            error = "描述不能为空！";
            return false;
        }

        if (descriptionInputField.inputString.Contains(',')) {
            error = "描述不能有半形逗号";
            return false;
        }

        return true;
    }
}
