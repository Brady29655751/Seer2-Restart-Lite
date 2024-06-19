using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.UI;
using SimpleFileBrowser;

public static class SaveSystem
{
    public const int MAX_SAVE_COUNT = 3;
    public static string savePath => Application.persistentDataPath + "/save" + Player.instance.gameDataId + ".xml";

    public static void SaveData() {
        SaveData(Player.instance.gameData);
    }

    public static void SaveData(GameData data, int id = -1) {
        id = (id == -1) ? Player.instance.gameDataId : id;
        string path = "save" + id.ToString();
        SaveXML(data, path);
    }

    public static GameData LoadData(int id = 0) {
        GameData data = null;
        string path = "save" + id.ToString();
        data = LoadXML<GameData>(path);
        if (data == null) {
            data = GameData.GetDefaultData(2000, 0);
            SaveData(data, id);
            
            Debug.Log("Save file not found in " + path);
            Debug.Log("Using default data.");
        }
        return data;
    }

    public static void SaveXML(object item, string path) {
        string XmlPath = Application.persistentDataPath + "/" + path + ".xml";
        using (StreamWriter writer = new StreamWriter(XmlPath)) {
            XmlSerializer serializer = new XmlSerializer(item.GetType());
            serializer.Serialize(writer.BaseStream, item);
            writer.Close();
        };
    }

    public static T LoadXML<T>(string path) {
        string XmlPath = Application.persistentDataPath + "/" + path + ".xml";
        if (!File.Exists(XmlPath))
            return default(T);
        
        using (StreamReader reader = new StreamReader(XmlPath)) {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            T deserialized = (T)serializer.Deserialize(reader.BaseStream);
            reader.Close();
            return deserialized;
        };
    }

    public static bool TrySaveXML(object item, string XmlPath, out string error) {
        try {
            using (StringWriter writer = new StringWriter()) {
                XmlSerializer serializer = new XmlSerializer(item.GetType());
                serializer.Serialize(writer, item);
                FileBrowserHelpers.WriteTextToFile(XmlPath, writer.ToString());
            };
            error = string.Empty;
            return true;
        } catch (Exception) {
            error = "档案导出失败";
            return false;
        }    
    }

    public static bool TryLoadXML<T>(string XmlPath, out T result, out string error) {
        if (!FileBrowserHelpers.FileExists(XmlPath)) {
            error = "档案不存在";
            result = default(T);
            return false;
        }

        try {
            string docs = FileBrowserHelpers.ReadTextFromFile(XmlPath);
            using (TextReader reader = new StringReader(docs)) {
                XmlSerializer serializer = new XmlSerializer(typeof(T));
                result = (T)serializer.Deserialize(reader);
            };
            error = string.Empty;
            return true;
        } catch (Exception) {
            error = "档案格式不符";
            result = default(T);
            return false;
        }
    }
    
    public static bool TryLoadAllBytes(string path, out byte[] bytes) {
        try {
            bytes = FileBrowserHelpers.ReadBytesFromFile(path);
        } catch (Exception) {
            bytes = null;
            return false;
        }
        return true;
    }

    public static bool TryCreateMod(out string error) {
        error = string.Empty;

        if (IsModExists()) {
            error = "创建失败，已有mod存在";
            return false;
        }

        try {
            var modPath = FileBrowserHelpers.CreateFolderInDirectory(Application.persistentDataPath, "Mod");

            var emblemPath = FileBrowserHelpers.CreateFolderInDirectory(modPath, "Emblems");
            var petPath = FileBrowserHelpers.CreateFolderInDirectory(modPath, "Pets");
            var petIconPath = FileBrowserHelpers.CreateFolderInDirectory(petPath, "icon");
            var petPetPath = FileBrowserHelpers.CreateFolderInDirectory(petPath, "pet");
            var petBattlePath = FileBrowserHelpers.CreateFolderInDirectory(petPath, "battle");

            var skillPath = FileBrowserHelpers.CreateFolderInDirectory(modPath, "Skills");
            var buffPath = FileBrowserHelpers.CreateFolderInDirectory(modPath, "Buffs");
            var itemPath = FileBrowserHelpers.CreateFolderInDirectory(modPath, "Items");
            var activityPath = FileBrowserHelpers.CreateFolderInDirectory(modPath, "Activities");

            var bgmPath = FileBrowserHelpers.CreateFolderInDirectory(modPath, "BGM");
            var bgmFxPath = FileBrowserHelpers.CreateFolderInDirectory(bgmPath, "fx");

            var mapPath = FileBrowserHelpers.CreateFolderInDirectory(modPath, "Maps");
            var mapBgPath = FileBrowserHelpers.CreateFolderInDirectory(mapPath, "bg");
            var mapFightBgPath = FileBrowserHelpers.CreateFolderInDirectory(mapPath, "fightBg");
            var mapPathPath = FileBrowserHelpers.CreateFolderInDirectory(mapPath, "path");
            var mapMinePath = FileBrowserHelpers.CreateFolderInDirectory(mapPath, "mine");

            var npcPath = FileBrowserHelpers.CreateFolderInDirectory(modPath, "Npc");
        } catch (Exception) {
            error = "mod资料夹创建失败";
            return false;
        }
        return true;
    }

    public static bool IsModExists() {
        return FileBrowserHelpers.DirectoryExists(Application.persistentDataPath + "/Mod");
    }

    private static bool TryCreateFile(string dirPath, string fileName, out string filePath) {
        filePath = dirPath + fileName;
        try {
            if (!FileBrowserHelpers.FileExists(filePath)) {
                string parentDirectory = FileBrowserHelpers.GetDirectoryName(filePath);
                filePath = FileBrowserHelpers.CreateFileInDirectory(parentDirectory, fileName);   
            }   
        } catch (Exception) {
            return false;
        }
        return true;
    }

    public static bool TryImportMod(string importPath) {
        try {
            var modPath = Application.persistentDataPath + "/Mod";
            FileBrowserHelpers.CopyDirectory(importPath, modPath);   
        } catch (Exception) {
            return false;
        }
        return true;
    }

    public static bool TryExportMod(string exportPath) {
        try {
            var modPath = Application.persistentDataPath + "/Mod";
            var dirPath = FileBrowserHelpers.CreateFolderInDirectory(exportPath, "Mod");  
            FileBrowserHelpers.CopyDirectory(modPath, dirPath);  
        } catch (Exception) {
            return false;
        }
        return true;
    }

    public static bool TryDeleteMod() {
        try {
            var modPath = Application.persistentDataPath + "/Mod";
            if (!IsModExists())
                return true;

            FileBrowserHelpers.DeleteDirectory(modPath);
            
            // Remove all mod pet in all game data.
            for (int id = 0; id < MAX_SAVE_COUNT; id++) {
                var data = LoadData(id);

                data.petStorage.RemoveAll(x => PetInfo.IsMod(x?.id ?? 0));
                data.petBag = data.petBag.Where(x => !PetInfo.IsMod(x?.id ?? 0)).ToArray();
                Array.Resize(ref data.petBag, 6);
                data.pvpPetTeam.RemoveAll(x => x.value.Any(y => PetInfo.IsMod(y?.id ?? 0)));

                data.activityStorage.RemoveAll(x => ActivityInfo.IsMod(x.id));
                data.itemStorage.RemoveAll(x => ItemInfo.IsMod(x.id));

                SaveData(data, id);
            }
        } catch (Exception) {
            return false;
        }
        return true;
    }

    public static bool TrySaveBuffMod(BuffInfo info, byte[] iconBytes, Sprite iconSprite) {
        var buffPath = Application.persistentDataPath + "/Mod/Buffs/";
        try {
            if (!TryCreateFile(buffPath, "info.csv", out var infoPath))
                return false;

            if (!TryCreateFile(buffPath, "effect.csv", out var effectPath))
                return false;

            FileBrowserHelpers.WriteTextToFile(infoPath, "id,name,type,copyType,turn,options,description\n"); 
            FileBrowserHelpers.WriteTextToFile(effectPath, "id,timing,priority,target,condition,cond_option,effect,effect_option\n"); 

            foreach (var entry in Database.instance.buffInfoDict) {
                if (!BuffInfo.IsMod(entry.Key))
                    continue;

                FileBrowserHelpers.AppendTextToFile(infoPath, entry.Value.GetRawInfoStringArray().ConcatToString(",") + "\n");
                FileBrowserHelpers.AppendTextToFile(effectPath, Effect.GetRawEffectListStringArray(entry.Key, entry.Value.effects).ConcatToString(",") + "\n");
            }

            if (iconBytes != null) {
                FileBrowserHelpers.WriteBytesToFile(buffPath + info.id + ".png", iconBytes);
                ResourceManager.instance.Set<Sprite>("Mod/Buffs/" + info.id, iconSprite);
            }

        } catch (Exception) {
            return false;
        }

        return true;
    }

    public static bool TryLoadBuffMod(out Dictionary<int, BuffInfo> buffDict) {
        buffDict = null;

        var buffPath = Application.persistentDataPath + "/Mod/Buffs/";
        try {
            string infoPath = buffPath + "info.csv";
            string effectPath = buffPath + "effect.csv";
            if ((!FileBrowserHelpers.FileExists(infoPath)) || (!FileBrowserHelpers.FileExists(effectPath)))
                return false;
            
            var data = ResourceManager.GetCSV(FileBrowserHelpers.ReadTextFromFile(infoPath));
            var effect = ResourceManager.GetCSV(FileBrowserHelpers.ReadTextFromFile(effectPath));
            
            buffDict = ResourceManager.instance.GetBuffInfo(data, effect);
        } catch (Exception) {
            return false;
        }
        return true;
    }

    public static bool TrySaveSkillMod(Skill info) {
        var skillPath = Application.persistentDataPath + "/Mod/Skills/";
        try {
            if (!TryCreateFile(skillPath, "info.csv", out var infoPath))
                return false;

            if (!TryCreateFile(skillPath, "effect.csv", out var effectPath))
                return false;

            FileBrowserHelpers.WriteTextToFile(infoPath, "id,name,element,type,power,anger,accuracy,option,description\n"); 
            FileBrowserHelpers.WriteTextToFile(effectPath, "id,timing,priority,target,condition,cond_option,effect,effect_option\n"); 

            foreach (var entry in Database.instance.skillDict) {
                if (!Skill.IsMod(entry.Key))
                    continue;

                FileBrowserHelpers.AppendTextToFile(infoPath, entry.Value.GetRawInfoStringArray().ConcatToString(",") + "\n");
                FileBrowserHelpers.AppendTextToFile(effectPath, Effect.GetRawEffectListStringArray(entry.Key, entry.Value.effects).ConcatToString(",") + "\n");
            }
        } catch (Exception) {
            return false;
        }

        return true;
    }

    public static bool TryLoadSkillMod(out Dictionary<int, Skill> skillDict) {
        skillDict = null;

        var skillPath = Application.persistentDataPath + "/Mod/Skills/";
        try {
            string infoPath = skillPath + "info.csv";
            string effectPath = skillPath + "effect.csv";
            if ((!FileBrowserHelpers.FileExists(infoPath)) || (!FileBrowserHelpers.FileExists(effectPath)))
                return false;

            var data = ResourceManager.GetCSV(FileBrowserHelpers.ReadTextFromFile(infoPath));
            var effect = ResourceManager.GetCSV(FileBrowserHelpers.ReadTextFromFile(effectPath));

            skillDict = ResourceManager.instance.GetSkill(data, effect);
        } catch (Exception) {
            return false;
        }
        return true;
    }

    public static bool TrySavePetMod(PetInfo info, Dictionary<string, byte[]> bytesDict, Dictionary<string, Sprite> spriteDict) {
        var petPath = Application.persistentDataPath + "/Mod/Pets/";
        var emblemPath = Application.persistentDataPath + "/Mod/Emblems/";
        var iconBytes = bytesDict.Get("icon", null);
        var battleBytes = bytesDict.Get("battle", null);
        var emblemBytes = bytesDict.Get("emblem", null);

        try {
            if (!TryCreateFile(petPath, "basic.csv", out var basicPath))
                return false;

            if (!TryCreateFile(petPath, "feature.csv", out var featurePath))
                return false;

            if (!TryCreateFile(petPath, "exp.csv", out var expPath))
                return false;

            if (!TryCreateFile(petPath, "skill.csv", out var skillPath))
                return false;

            if (!TryCreateFile(petPath, "ui.csv", out var uiPath))
                return false;

            FileBrowserHelpers.WriteTextToFile(basicPath, "id,baseId,name,element,baseStatus,gender,baseHeight/baseWeight,description,habitat,linkId\n"); 
            FileBrowserHelpers.WriteTextToFile(featurePath, "baseId,featureName,featureDescription,emblemName,emblemDescription\n"); 
            FileBrowserHelpers.WriteTextToFile(expPath, "id,expType,evolvePetId,evolveLevel,beatExpParam\n"); 
            FileBrowserHelpers.WriteTextToFile(skillPath, "id,ownSkill,learnLevel\n"); 
            FileBrowserHelpers.WriteTextToFile(uiPath, "id,baseId,skinChoice,options\n"); 

            foreach (var entry in Database.instance.petInfoDict) {
                if (!PetInfo.IsMod(entry.Key))
                    continue;

                FileBrowserHelpers.AppendTextToFile(basicPath, entry.Value.basic.GetRawInfoStringArray().ConcatToString(",") + "\n");
                FileBrowserHelpers.AppendTextToFile(expPath, entry.Value.exp.GetRawInfoStringArray().ConcatToString(",") + "\n");
                FileBrowserHelpers.AppendTextToFile(skillPath, entry.Value.skills.GetRawInfoStringArray().ConcatToString(",") + "\n");

                // Do not write ui when no skin info.
                var uiRawString = entry.Value.ui.GetRawInfoStringArray();
                if (uiRawString != null)
                    FileBrowserHelpers.AppendTextToFile(uiPath, uiRawString.ConcatToString(",") + "\n");
            }

            foreach (var entry in Database.instance.featureInfoDict) {
                if ((!PetInfo.IsMod(entry.Key)) || (entry.Value == null))
                    continue;

                FileBrowserHelpers.AppendTextToFile(featurePath, entry.Value.GetRawInfoStringArray().ConcatToString(",") + "\n");
            }

            // Write image bytes.
            if (iconBytes != null) {
                FileBrowserHelpers.WriteBytesToFile(petPath + "/icon/" + info.id + ".png", iconBytes);
                ResourceManager.instance.Set<Sprite>("Mod/Pets/icon/" + info.id, spriteDict.Get("icon"));
            }

            if (battleBytes != null) {
                FileBrowserHelpers.WriteBytesToFile(petPath + "/battle/" + info.id + ".png", battleBytes);
                ResourceManager.instance.Set<Sprite>("Mod/Pets/battle/" + info.id, spriteDict.Get("battle"));
            }

            if ((info.id == info.baseId) && (emblemBytes != null)) {
                FileBrowserHelpers.WriteBytesToFile(emblemPath + info.id + ".png", emblemBytes);
                ResourceManager.instance.Set<Sprite>("Mod/Emblems/" + info.id, spriteDict.Get("emblem"));
            }

            // Remove all pet with same id in game data.
            for (int id = 0; id < MAX_SAVE_COUNT; id++) {
                var data = LoadData(id);

                data.petStorage.RemoveAll(x => (x?.id ?? 0) == info.id);
                data.petBag = data.petBag.Where(x => (x?.id ?? 0) != info.id).ToArray();
                Array.Resize(ref data.petBag, 6);
                data.pvpPetTeam.RemoveAll(x => x.value.Any(y => (y?.id ?? 0) == info.id));
                SaveData(data, id);
            }

        } catch (Exception) {
            return false;
        }

        return true;
    }

    public static bool TryLoadPetMod(out Dictionary<int, PetInfo> petDict, out Dictionary<int, PetFeatureInfo> featureDict) {
        petDict = new Dictionary<int, PetInfo>();
        featureDict = new Dictionary<int, PetFeatureInfo>();

        try {
            // Init path.
            var petPath = Application.persistentDataPath + "/Mod/Pets/";
            var basicPath = petPath + "basic.csv";
            var featurePath = petPath + "feature.csv";
            var expPath = petPath + "exp.csv";
            var skillPath = petPath + "skill.csv";
            var uiPath = petPath + "ui.csv";

            // Check files exist.
            if ((!FileBrowserHelpers.FileExists(basicPath)) || (!FileBrowserHelpers.FileExists(featurePath)) ||
                (!FileBrowserHelpers.FileExists(expPath)) || (!FileBrowserHelpers.FileExists(skillPath)) ||
                (!FileBrowserHelpers.FileExists(uiPath)))
                return false;


            // Get data.
            var basicData = ResourceManager.GetCSV(FileBrowserHelpers.ReadTextFromFile(basicPath));
            var featureData = ResourceManager.GetCSV(FileBrowserHelpers.ReadTextFromFile(featurePath));
            var expData = ResourceManager.GetCSV(FileBrowserHelpers.ReadTextFromFile(expPath));
            var skillData = ResourceManager.GetCSV(FileBrowserHelpers.ReadTextFromFile(skillPath));
            var uiData = ResourceManager.GetCSV(FileBrowserHelpers.ReadTextFromFile(uiPath));

            // Set infos.
            List<PetBasicInfo> basicInfo = ResourceManager.instance.GetPetBasicInfo(basicData);
            Dictionary<int, PetFeatureInfo> featureInfo = ResourceManager.instance.GetPetFeatureInfo(featureData);
            Dictionary<int, PetExpInfo> expInfo = ResourceManager.instance.GetPetExpInfo(expData);
            Dictionary<int, PetSkillInfo> skillInfo = ResourceManager.instance.GetPetSkillInfo(skillData);
            Dictionary<int, PetUIInfo> uiInfo = ResourceManager.instance.GetPetUIInfo(uiData);

            // Load to dict.
            for (int i = 0; i < basicInfo.Count; i++) {
                var basic = basicInfo[i];
                var ui = uiInfo.Get(basic.id, new PetUIInfo(basic.id, basic.baseId));
                PetInfo info = new PetInfo(basic, featureInfo.Get(ui.defaultFeatureId), expInfo.Get(basic.id), new PetTalentInfo(), skillInfo.Get(basic.id), ui);
                petDict.Set(info.id, info);
            }

            featureDict = featureInfo;            

        } catch (Exception) {
            return false;
        }

        return true;
    }

    public static bool TryLoadMapMod(int id, Action<Map> onSuccess = null, Action<string> onFail = null) {
        try {
            var mapPath = Application.persistentDataPath + "/Mod/Maps/" + id + ".xml";
            if (!FileBrowserHelpers.FileExists(mapPath)) {
                onFail?.Invoke("找不到对应的地图档案");
                return false;
            }

            Map map = ResourceManager.GetXML<Map>(FileBrowserHelpers.ReadTextFromFile(mapPath));
            
            ResourceManager.instance.StartCoroutine(TryLoadMapResourcesMod(map, onSuccess, onFail));
        } catch (Exception) {
            onFail?.Invoke("加载地图资料发生错误");
            return false;
        }

        return true;
    }

    private static IEnumerator TryLoadMapResourcesMod(Map map, Action<Map> onSuccess = null, Action<string> onFail = null) {
        int resId = (map.resId == 0) ? map.id : map.resId;
        bool isMod = Map.IsMod(resId);
        int pathResId = isMod ? resId : Mathf.Abs(resId);

        var modPath = Application.persistentDataPath + "/Mod/";
        int doneRequestCount = 0;

        AudioClip bgm = null, fx = null;
        string error = string.Empty;

        // Audio only accepts mp3
        RequestManager.instance.DownloadAudioClip("file://" + modPath + "BGM/" + map.music.category + "/" + map.music.bgm + ".mp3", 
            (clip) => { bgm = clip; doneRequestCount++; }, (message) => { error = message; doneRequestCount++; });

        if (string.IsNullOrEmpty(map.music.fx))
            doneRequestCount++;
        else {
            RequestManager.instance.DownloadAudioClip("file://" + modPath + "BGM/fx/" + map.music.fx + ".mp3", 
            (clip) => { fx = clip; doneRequestCount++; }, (message) => { error = message; doneRequestCount++; });
        }

        Sprite bg = ResourceManager.instance.GetLocalAddressables<Sprite>("Maps/bg/" + resId, isMod);
        Sprite pathSprite = ResourceManager.instance.GetLocalAddressables<Sprite>("Maps/path/" + pathResId, isMod);

        while (doneRequestCount < 2)
            yield return null;

        if (!string.IsNullOrEmpty(error)) {
            onFail?.Invoke(error);
            yield break;
        }

        MapResources mapResources = new MapResources(bg, pathSprite, bgm, fx);
        map.SetResources(mapResources);

        onSuccess?.Invoke(map);
    }

    public static bool TryLoadActivityMod(out Dictionary<string, ActivityInfo> activityDict) {
        activityDict = null;

        var activityPath = Application.persistentDataPath + "/Mod/Activities/";
        try {
            string infoPath = activityPath + "info.csv";
            if (!FileBrowserHelpers.FileExists(infoPath))
                return false;

            var data = ResourceManager.GetCSV(FileBrowserHelpers.ReadTextFromFile(infoPath));

            activityDict = ResourceManager.instance.GetActivityInfo(data);
        } catch (Exception) {
            return false;
        }
        return true;
    }

    public static bool TryLoadItemMod(out Dictionary<int, ItemInfo> itemDict) {
        itemDict = null;

        var itemPath = Application.persistentDataPath + "/Mod/Items/";
        try {
            string infoPath = itemPath + "info.csv";
            string effectPath = itemPath + "effect.csv";
            if ((!FileBrowserHelpers.FileExists(infoPath)) || (!FileBrowserHelpers.FileExists(effectPath)))
                return false;

            var data = ResourceManager.GetCSV(FileBrowserHelpers.ReadTextFromFile(infoPath));
            var effect = ResourceManager.GetCSV(FileBrowserHelpers.ReadTextFromFile(effectPath));

            itemDict = ResourceManager.instance.GetItemInfo(data, effect);
        } catch (Exception) {
            return false;
        }
        return true;
    }

    public static bool TryLoadPanelMod(string panelName, out PanelData panelData) {
        panelData = null;

        var panelPath = Application.persistentDataPath + "/Mod/Panel/" + panelName + "/panel.xml";
        try {
            if (!FileBrowserHelpers.FileExists(panelPath))
                return false;

            panelData = ResourceManager.GetXML<PanelData>(FileBrowserHelpers.ReadTextFromFile(panelPath));
        } catch (Exception) {
            return false;
        }

        return true;
    }

    public static bool TryLoadElementMod() {
        var elementPath = Application.persistentDataPath + "/Mod/Elements/";
        var spritePath = ResourceManager.instance.spritePath + "Elements/";

        try {
            string infoPath = elementPath + "info.csv";
            
            if (!FileBrowserHelpers.FileExists(infoPath))
                return false;

            var data = ResourceManager.GetCSV(FileBrowserHelpers.ReadTextFromFile(infoPath));

            ResourceManager.instance.GetElementInfo(data);

            for (int i = 0; i < PetElementSystem.elementNum; i++) {
                if (!SaveSystem.TryLoadAllBytes(elementPath + i + ".png", out var bytes))
                    continue;

                if (!SpriteSet.TryCreateSpriteFromBytes(bytes, out var sprite))
                    continue;

                ResourceManager.instance.Set<Sprite>(spritePath + i, sprite);
            }
        } catch (Exception) {
            return false;
        }

        return true;
    }
}
