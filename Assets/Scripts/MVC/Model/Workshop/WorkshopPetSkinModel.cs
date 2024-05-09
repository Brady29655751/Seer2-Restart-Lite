using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SimpleFileBrowser;

public class WorkshopPetSkinModel : Module
{
    [SerializeField] IInputField optionInputField;
    public string options => optionInputField.inputString;

    public string spriteType { get; private set; }
    public Dictionary<string, byte[]> bytesDict = new Dictionary<string, byte[]>() {
        { "icon", null },   { "emblem", null }, { "battle", null },
    };
    public Dictionary<string, Sprite> spriteDict = new Dictionary<string, Sprite>() {
        { "icon", null },   { "emblem", null }, { "battle", null },
    };

    private Action<Sprite> onUploadSpriteCallback;

    public List<int> skinList = new List<int>();

    public PetUIInfo GetPetUIInfo(int id, int baseId) {
        var uiInfo = new PetUIInfo(id, baseId);
        uiInfo.specialSkinList = skinList;
        uiInfo.options.ParseOptions(options);
        return uiInfo;
    }

    public void SetPetUIInfo(PetUIInfo uiInfo) {
        skinList = uiInfo.specialSkinList;
        optionInputField.SetInputString(uiInfo.GetRawOptionString());

        foreach (var key in bytesDict.Keys.ToList())
            bytesDict.Set(key, null);

        foreach (var key in spriteDict.Keys.ToList())
            spriteDict.Set(key, null);
    }

    public void OnUploadSprite(string type, Action<Sprite> callback = null) {
        spriteType = type;
        onUploadSpriteCallback = callback;

        var filter = new FileBrowser.Filter("图片", ".png");
        FileBrowser.SetFilters(false, filter);
        FileBrowser.ShowLoadDialog(OnUploadSpriteSuccess, OnUploadSpriteCancel, FileBrowser.PickMode.Files, title: "选择要上传的图片");
    }

    private void OnUploadSpriteSuccess(string[] paths) {
        if (!SaveSystem.TryLoadAllBytes(paths[0], out var bytes)) {
            Hintbox.OpenHintboxWithContent("图片载入失败", 16);
            return;
        }
        if (!SpriteSet.TryCreateSpriteFromBytes(bytes, out var sprite)) {
            Hintbox.OpenHintboxWithContent("创造精灵图标失败", 16);
            return;
        }
        bytesDict.Set(spriteType, bytes);
        spriteDict.Set(spriteType, sprite);
        
        onUploadSpriteCallback?.Invoke(spriteDict[spriteType]);
    }

    private void OnUploadSpriteCancel() {
        // pass.
    }

    public void OnClearSprite(string type) {
        bytesDict.Set(type, null);
        spriteDict.Set(type, null);
    }

    public bool VerifyDIYPetSkin(int id, int baseId, out string error) {
        error = string.Empty;

        if (!VerifyOptions(out error))
            return false;

        // If already exists (edit mode), no need to check others.
        var petInfo = Pet.GetPetInfo(id);
        if (petInfo != null)
            return true;

        // If not exists (create mode), check default skin first.
        var defaultSkinId = GetPetUIInfo(id, baseId).defaultSkinId;
        if (id == defaultSkinId)
            return true;

        if ((bytesDict.Get("icon", null) == null) || (bytesDict.Get("battle", null) == null)) {
            error = "请上传头像和精灵图片！";
            return false;
        }

        if ((id == baseId) && (bytesDict.Get("emblem", null) == null)) {
            error = "请上传纹章图片！";
            return false;
        }

        return true;
    }

    private bool VerifyOptions(out string error) {
        error = string.Empty;

        if (options.Contains(',')) {
            error = "【其他】部分不能有半形逗号";
            return false;
        }

        var dict = new Dictionary<string, string>();
        try {
            dict.ParseOptions(options);
        } catch (Exception) {
            error = "【其他】部分有重复或残缺的自定义选项";
            return false;
        }

        if (!int.TryParse(dict.Get("default_skin", "0"), out var defaultSkinId)) {
            error = "【其他】自定义选项的【默认皮肤】序号需为整数";
            return false;
        }

        if (defaultSkinId == 0)
            return true;

        var resInfo = Pet.GetPetInfo(defaultSkinId);
        if ((resInfo == null) || (resInfo.ui.id != resInfo.ui.defaultSkinId)) {
            error = "【其他】的【默认皮肤】引用的序号图片不存在";
            return false;
        }

        return true;
    }

}
