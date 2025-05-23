using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorkshopPetBasicModel : Module
{
    [SerializeField] private IInputField idInputField, nameInputField, genderInputField;
    [SerializeField] private IDropdown elementDropdown, subElementDropdown;
    [SerializeField] private IInputField heightInputField, weightInputField;
    [SerializeField] private List<IInputField> baseStatusInputFieldList;
    [SerializeField] private IInputField descriptionInputField;

    public int id => int.Parse(idInputField.inputString);
    public string petName => nameInputField.inputString;
    public Element element => (Element)(elementDropdown.value);
    public Element subElement => (Element)(subElementDropdown.value);
    public string gender => genderInputField.inputString;
    public int height => int.Parse(heightInputField.inputString);
    public int weight => int.Parse(weightInputField.inputString);
    public Status baseStatus => new Status(baseStatusInputFieldList.Select(x => float.Parse(x.inputString)));

    public string description => descriptionInputField.inputString;

    protected override void Awake() {
        // if (!PetElementSystem.IsMod())
        //     return;

        elementDropdown.SetDropdownOptions(PetElementSystem.elementNameList);
        subElementDropdown.SetDropdownOptions(PetElementSystem.elementNameList);
    }

    public PetBasicInfo GetPetBasicInfo(int baseId) {
        return new PetBasicInfo(id, baseId, petName, element, subElement, baseStatus,
            gender, height, weight, description, "创意工坊", "Workshop");
    }

    public void SetPetBasicInfo(PetBasicInfo basicInfo) {
        idInputField.SetInputString(basicInfo.id.ToString());
        nameInputField.SetInputString(basicInfo.name);

        elementDropdown.value = (int)basicInfo.element;
        subElementDropdown.value = (int)basicInfo.subElement;
        genderInputField.SetInputString(basicInfo.rawGender);

        heightInputField.SetInputString(basicInfo.baseHeight.ToString());
        weightInputField.SetInputString(basicInfo.baseWeight.ToString());

        for (int i = 0; i < baseStatusInputFieldList.Count; i++)
            baseStatusInputFieldList[i].SetInputString(basicInfo.baseStatus[i].ToString());
        
        descriptionInputField.SetInputString(basicInfo.description);
    }

    public bool VerifyDIYPetBasic(int baseId, out string error) {
        error = string.Empty;

        if (!VerifyId(out error))
            return false;

        if (!VerifyName(out error))
            return false;

        if (!VerifyGender(out error))
            return false;

        if (!VerifyHeightAndWeight(out error))
            return false;

        if (!VerifyBaseStatus(out error))
            return false;

        if (!VerifyDescription(out error))
            return false;

        if (!VerifyBaseId(baseId, out error))
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

        if (id > -1) {
            error = "序号需小于等于-1";
            return false;
        }

        return true;
    }

    private bool VerifyBaseId(int baseId, out string error) {
        error = string.Empty;

        if (id == baseId)
            return true;

        if (baseId == 0) {
            error = "基础型态序号不能为空！";
            return false;
        }

        if (Pet.GetPetInfo(baseId) == null) {
            error = "基础型态序号不存在";
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

    private bool VerifyGender(out string error) {
        error = string.Empty;

        if (string.IsNullOrEmpty(genderInputField.inputString)) {
            error = "性别不能为空！";
            return false;
        }

        if (!PetBasicInfo.TryParseGender(genderInputField.inputString, out _, out _)) {
            error = "性别格式错误！";
            return false;
        }

        return true;
    }

    private bool VerifyHeightAndWeight(out string error) {
        error = string.Empty;

        if (string.IsNullOrEmpty(heightInputField.inputString) || 
            string.IsNullOrEmpty(weightInputField.inputString)) {
            error = "身高、体重不能为空！";
            return false;
        }

        if ((height < 0) || (weight < 0)) {
            error = "身高、体重不能为负数！";
            return false;
        }

        return true;
    }

    private bool VerifyBaseStatus(out string error) {
        error = string.Empty;

        if (baseStatusInputFieldList.Exists(x => string.IsNullOrEmpty(x.inputString))) {
            error = "种族值少写了一部份！";
            return false;
        }

        if (baseStatus.Count(x => x < 0) > 0) {
            error = "种族值不能为负数";
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
