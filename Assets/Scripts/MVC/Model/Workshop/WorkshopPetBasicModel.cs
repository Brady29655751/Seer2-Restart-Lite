using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorkshopPetBasicModel : Module
{
    [SerializeField] private IInputField idInputField, nameInputField;
    [SerializeField] private IDropdown elementDropdown, genderDropdown;
    [SerializeField] private IInputField heightInputField, weightInputField;
    [SerializeField] private List<IInputField> baseStatusInputFieldList;
    [SerializeField] private IInputField descriptionInputField;

    public int id => int.Parse(idInputField.inputString);
    public string petName => nameInputField.inputString;
    public Element element => (Element)(elementDropdown.value);
    public int gender => genderDropdown.value - 1;
    public int height => int.Parse(heightInputField.inputString);
    public int weight => int.Parse(weightInputField.inputString);
    public Status baseStatus => new Status(baseStatusInputFieldList.Select(x => float.Parse(x.inputString)));

    public string description => descriptionInputField.inputString;


    public PetBasicInfo GetPetBasicInfo(int baseId) {
        return new PetBasicInfo(id, baseId, petName, element, baseStatus,
            gender, height, weight, description, "创意工坊", "none");
    }

    public bool VerifyDIYPetBasic(int baseId, out string error) {
        error = string.Empty;

        if (!VerifyId(out error))
            return false;

        if (!VerifyName(out error))
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

        if (Pet.GetPetInfo(id) != null) {
            error = "此序号已被占用，不可重复！";
            return false;
        }

        if (id > -13) {
            error = "序号需小于等于-13";
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

        if (baseStatus.Count(x => !x.IsWithin(0, 255)) > 0) {
            error = "种族值必须介于0到255之间";
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
