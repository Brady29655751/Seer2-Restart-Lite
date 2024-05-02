using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;
using UnityEngine;
using SimpleFileBrowser;

public static class SaveSystem
{
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

    public static bool TryCreateMod() {
        try {
            if (FileBrowserHelpers.FileExists(Application.persistentDataPath + "/Mod"))
                return true;

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
        } catch (Exception) {
            return false;
        }
        return true;
    }

    private static bool TryCreateFile(string dirPath, string fileName, string header, out string filePath) {
        filePath = dirPath + fileName;
        try {
            if (!FileBrowserHelpers.FileExists(filePath)) {
                string parentDirectory = FileBrowserHelpers.GetDirectoryName(filePath);
                filePath = FileBrowserHelpers.CreateFileInDirectory(parentDirectory, fileName);
                FileBrowserHelpers.WriteTextToFile(filePath, header);    
            }
        } catch (Exception) {
            return false;
        }
        return true;
    }

    public static bool TrySaveBuffMod(BuffInfo info, byte[] iconBytes) {
        var buffPath = Application.persistentDataPath + "/Mod/Buffs/";
        try {
            if (!TryCreateFile(buffPath, "info.csv", "id,name,type,copyType,turn,options,description\n", out var infoPath))
                return false;

            if (!TryCreateFile(buffPath, "effect.csv", "id,timing,priority,target,condition,cond_option,effect,effect_option\n", out var effectPath))
                return false;

            FileBrowserHelpers.AppendTextToFile(infoPath, info.GetRawInfoStringArray().ConcatToString(",") + "\n");
            FileBrowserHelpers.AppendTextToFile(effectPath, Effect.GetRawEffectListStringArray(info.id, info.effects).ConcatToString(",") + "\n");

            if (iconBytes != null)
                FileBrowserHelpers.WriteBytesToFile(buffPath + info.id + ".png", iconBytes);

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
            if (!TryCreateFile(skillPath, "info.csv", "id,name,element,type,power,anger,accuracy,option,description\n", out var infoPath))
                return false;

            if (!TryCreateFile(skillPath, "effect.csv", "id,timing,priority,target,condition,cond_option,effect,effect_option\n", out var effectPath))
                return false;

            FileBrowserHelpers.AppendTextToFile(infoPath, info.GetRawInfoStringArray().ConcatToString(",") + "\n");
            FileBrowserHelpers.AppendTextToFile(effectPath, Effect.GetRawEffectListStringArray(info.id, info.effects).ConcatToString(",") + "\n");
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

    public static bool TrySavePetMod(PetInfo info, Dictionary<string, byte[]> bytesDict) {
        var petPath = Application.persistentDataPath + "/Mod/Pets/";
        var emblemPath = Application.persistentDataPath + "/Mod/Emblems/";
        var iconBytes = bytesDict.Get("icon", null);
        var battleBytes = bytesDict.Get("battle", null);
        var emblemBytes = bytesDict.Get("emblem", null);

        try {
            if (!TryCreateFile(petPath, "basic.csv", "id,baseId,name,element,baseStatus,gender,baseHeight/baseWeight,description,habitat,linkId\n", out var basicPath))
                return false;

            if (!TryCreateFile(petPath, "feature.csv", "baseId,featureName,featureDescription,emblemName,emblemDescription\n", out var featurePath))
                return false;

            if (!TryCreateFile(petPath, "exp.csv", "id,expType,evolvePetId,evolveLevel,beatExpParam\n", out var expPath))
                return false;

            if (!TryCreateFile(petPath, "skill.csv", "id,ownSkill,learnLevel\n", out var skillPath))
                return false;

            if (!TryCreateFile(petPath, "ui.csv", "id,baseId,skinChoice,options\n", out var uiPath))
                return false;

            FileBrowserHelpers.AppendTextToFile(basicPath, info.basic.GetRawInfoStringArray().ConcatToString(",") + "\n");

            // Only write feature when this is a base pet.
            if (info.id == info.baseId)
                FileBrowserHelpers.AppendTextToFile(featurePath, info.feature.GetRawInfoStringArray().ConcatToString(",") + "\n");
            
            FileBrowserHelpers.AppendTextToFile(expPath, info.exp.GetRawInfoStringArray().ConcatToString(",") + "\n");
            FileBrowserHelpers.AppendTextToFile(skillPath, info.skills.GetRawInfoStringArray().ConcatToString(",") + "\n");

            // Do not write ui when no skin info.
            var uiRawString = info.ui.GetRawInfoStringArray();
            if (uiRawString != null)
                FileBrowserHelpers.AppendTextToFile(skillPath, uiRawString.ConcatToString(",") + "\n");

            // Write image bytes.
            if (iconBytes != null)
                FileBrowserHelpers.WriteBytesToFile(petPath + "/icon/" + info.id + ".png", iconBytes);

            if (battleBytes != null)
                FileBrowserHelpers.WriteBytesToFile(petPath + "/battle/" + info.id + ".png", battleBytes);

            if ((info.id == info.baseId) && (emblemBytes != null))
                FileBrowserHelpers.WriteBytesToFile(emblemPath + info.id + ".png", emblemBytes);

        } catch (Exception) {
            return false;
        }

        return true;
    }

    public static bool TryLoadPetMod(out Dictionary<int, PetInfo> petDict) {
        petDict = new Dictionary<int, PetInfo>();

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
                PetInfo info = new PetInfo(basic, featureInfo.Get(basic.baseId), expInfo.Get(basic.id), new PetTalentInfo(), skillInfo.Get(basic.id), uiInfo.Get(basic.id, new PetUIInfo(basic.id, basic.baseId)));
                petDict.Set(info.id, info);
            }            

        } catch (Exception) {
            Debug.Log("holy shit");
            return false;
        }

        return true;
    }
}
