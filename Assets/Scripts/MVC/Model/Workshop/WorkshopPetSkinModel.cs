using System;
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

}
