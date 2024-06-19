using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SimpleFileBrowser;

public class WorkshopBuffModel : Module
{
    [SerializeField] private IInputField idInputField, nameInputField;
    [SerializeField] private IDropdown typeDropdown, copyDropdown;
    [SerializeField] private IInputField turnInputField;
    [SerializeField] private Toggle keepToggle, inheritToggle, autoRemoveToggle;
    [SerializeField] private IInputField descriptionInputField, optionInputField;

    public BuffInfo buffInfo => GetBuffInfo();
    public int id => int.Parse(idInputField.inputString);
    public string buffName => nameInputField.inputString;
    public BuffType type => (BuffType)(typeDropdown.value + 1);
    public CopyHandleType copyHandleType => (CopyHandleType)(copyDropdown.value);
    public int turn => Mathf.Max(int.Parse(turnInputField.inputString), -1);
    public bool keep => keepToggle.isOn;
    public bool inherit => inheritToggle.isOn;
    public bool autoRemove => autoRemoveToggle.isOn;

    private byte[] iconBytes = null;
    public Sprite iconSprite;
    public string description => descriptionInputField.inputString?.Replace("\n", "[ENDL]") ?? string.Empty;
    public string descriptionPreview => Buff.GetBuffDescriptionPreview(description);
    public string options => optionInputField.inputString;
    public string optionsAll => ("keep=" + keep) + ("&inherit=" + inherit) + ("&auto_remove=" + autoRemove) + (string.IsNullOrEmpty(options) ? string.Empty : ("&" + options));

    public List<Effect> effectList = new List<Effect>();
    public event Action<Sprite> onUploadIconEvent;

    public BuffInfo GetBuffInfo() {
        var buffInfo = new BuffInfo(id, buffName, type, copyHandleType, turn, optionsAll, description);
        buffInfo.SetEffects(effectList.Select(x => new Effect(x)).ToList());
        return buffInfo;
    }

    public void SetBuffInfo(BuffInfo buffInfo) {
        idInputField.SetInputString(buffInfo.id.ToString());
        nameInputField.SetInputString(buffInfo.name);
        
        typeDropdown.value = ((int)buffInfo.type) - 1;
        copyDropdown.value = (int)buffInfo.copyHandleType;
        
        turnInputField.SetInputString(buffInfo.turn.ToString());

        keepToggle.isOn = buffInfo.keep;
        inheritToggle.isOn = buffInfo.inherit;
        autoRemoveToggle.isOn = buffInfo.autoRemove;

        descriptionInputField.SetInputString(buffInfo.description);

        var rawOptions = new Dictionary<string, string>(buffInfo.options);
        rawOptions.Remove("keep");
        rawOptions.Remove("inherit");
        rawOptions.Remove("auto_remove");
        optionInputField.SetInputString(rawOptions.Select(entry => entry.Key + "=" + entry.Value).ConcatToString("&"));

        effectList = buffInfo.effects.Select(x => new Effect(x)).ToList();

        iconBytes = null;
        iconSprite = null;
    }

    public void OnClearIcon() {
        iconBytes = null;
    }

    public void OnUploadIcon() {
        var filter = new FileBrowser.Filter("图片", ".png");
        FileBrowser.SetFilters(false, filter);
        FileBrowser.ShowLoadDialog(OnUploadIconSuccess, OnUploadIconCancel, FileBrowser.PickMode.Files, title: "选择要上传的图片");
    }

    private void OnUploadIconSuccess(string[] paths) {
        if (!SaveSystem.TryLoadAllBytes(paths[0], out iconBytes)) {
            Hintbox.OpenHintboxWithContent("图片载入失败", 16);
            return;
        }

        if (!SpriteSet.TryCreateSpriteFromBytes(iconBytes, out iconSprite)) {
            Hintbox.OpenHintboxWithContent("创造印记图标失败", 16);
            return;
        }

        onUploadIconEvent?.Invoke(iconSprite);
    }

    private void OnUploadIconCancel() {
        // pass.
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

    public bool CreateDIYBuff() {
        var originalBuffInfo = Buff.GetBuffInfo(buffInfo.id);
        Database.instance.buffInfoDict.Set(buffInfo.id, buffInfo);
        if (SaveSystem.TrySaveBuffMod(buffInfo, iconBytes, iconSprite))    
            return true;
        
        // rollback
        Database.instance.buffInfoDict.Set(buffInfo.id, originalBuffInfo);
        return false;
    }

    public bool VerifyDIYBuff(out string error) {
        if (!VerifyId(out error))
            return false;

        if (!VerifyName(out error))
            return false;

        if (!VerifyCopyType(out error))
            return false;

        if (!VerifyTurn(out error))
            return false;

        if (!VerifyOptions(out error))
            return false;

        if (!VerifyIcon(out error))
            return false;

        if (!VerifyDescription(out error))
            return false;

        error = string.Empty;
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

        if (type == BuffType.Feature) {
            if (id.IsWithin(90_0013, 99_9999))
                return true;

            error = "特性类的序号格式不符\n请点击序号右方的问号查看说明";
            return false;
        }

        if (type == BuffType.Emblem) {
            if (id.IsWithin(80_0013, 89_9999))
                return true;

            error = "纹章类的序号格式不符\n请点击序号右方的问号查看说明";
            return false;
        }

        if (type == BuffType.Weather) {
            if (id.IsWithin(51_0001, 59_9999))
                return true;

            error = "天气类的序号格式不符\n请点击序号右方的问号查看说明";
        }

        if (id > -10_0001) {
            error = "序号需小于等于-100001\n请点击序号右方的问号查看说明";
            return false;
        }
            
        return true;
    }

    private bool VerifyName(out string error) {
        error = string.Empty;

        if (string.IsNullOrEmpty(buffName)) {
            error = "名字不能为空！";
            return false;
        }

        if (buffName.Contains(',')) {
            error = "名字不能有半形逗号";
            return false;
        }

        /*
        if (((type == BuffType.Feature) || (type == BuffType.Emblem)) && (buffName.Length != 2)) {
            error = "特性、纹章类印记名称只能填写两个字";
            return false;
        }
        */

        return true;
    }

    private bool VerifyCopyType(out string error) {
        error = string.Empty;

        if ((type == BuffType.Feature) || (type == BuffType.Emblem)) {
            if (copyHandleType == CopyHandleType.Replace)
                return true;

            error = "特性、纹章类的覆盖必须是【覆盖旧印记】";
            return false;
        }

        return true;
    }

    private bool VerifyTurn(out string error) {
        error = string.Empty;

        if (string.IsNullOrEmpty(turnInputField.inputString)) {
            error = "回合不能为空！";
            return false;
        }

        if (!int.TryParse(turnInputField.inputString, out _)) {
            error = "回合必须为整数";
            return false;
        }

        if (turn == 0) {
            error = "回合不能为0！";
            return false;
        }

        if ((type == BuffType.Feature) || (type == BuffType.Emblem) || (type == BuffType.Mark)) {
            if (turn < 0)
                return true;

            error = "特性、纹章、标记类预设回合必须为永久";
            return false;
        }

        if (type == BuffType.TurnBased) {
            if (turn > 0)
                return true;

            error = "回合类预设回合不能为永久";
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

        if ((!int.TryParse(dict.Get("max_val", "1"), out var maxVal)) ||
            (!int.TryParse(dict.Get("min_val", "1"), out var minVal)) || 
            (maxVal <= 0) || (minVal <= 0)) 
        {
            error = "【其他】自定义选项的【最大值／最小值】\n必须为大于0的数字或不填写";
            return false;
        }

        if (!bool.TryParse(dict.Get("hide", "false"), out var hideOption)) {
            error = "【其他】自定义选项的【隐藏选项】\n必须为true或false或不填写";
            return false;
        }

        return true;
    }

    private bool VerifyIcon(out string error) {
        error = string.Empty;

        if ((type == BuffType.Feature) || (type == BuffType.Emblem))
            return true;

        var dict = new Dictionary<string, string>();
        dict.ParseOptions(optionsAll);

        var res = dict.Get("res", "0");

        if (!int.TryParse(res, out int resId)) {
            error = "【其他】的res自定义选项必须填写序号\n例如：res=3001";
            return false;
        }

        if (resId != 0) {
            var info = Buff.GetBuffInfo(resId);
            if ((info != null) && (info.id == info.resId))
                return true;
                
            error = "【其他】的res自定义选项引用的序号图案不存在";
            return false;
        }

        if ((Buff.GetBuffInfo(id) == null) && (iconBytes == null)) {
            error = "未指定buff图案或res自定义选项";
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
