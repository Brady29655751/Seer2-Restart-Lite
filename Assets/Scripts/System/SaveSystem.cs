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

    public static bool CreateMod() {
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

    public static bool TrySaveBuffMod(BuffInfo info, byte[] iconBytes) {
        var buffPath = Application.persistentDataPath + "/Mod/Buffs/";
        try {
            // Write icon.
            if (iconBytes != null)
                FileBrowserHelpers.WriteBytesToFile(buffPath + info.id + ".png", iconBytes);

            // Write info.
            string infoPath = buffPath + "info.csv";
            if (!FileBrowserHelpers.FileExists(infoPath)) {
                string parentDirectory = FileBrowserHelpers.GetDirectoryName(infoPath);
                infoPath = FileBrowserHelpers.CreateFileInDirectory(parentDirectory, "info.csv");
                FileBrowserHelpers.WriteTextToFile(infoPath, "id,name,type,copyType,turn,options,description\n");    
            }
            FileBrowserHelpers.AppendTextToFile(infoPath, info.GetRawInfoStringArray().ConcatToString(",") + "\n");

            // Write effect.
            string effectPath = buffPath + "effect.csv";
            if (!FileBrowserHelpers.FileExists(effectPath)) {
                string parentDirectory = FileBrowserHelpers.GetDirectoryName(effectPath);
                effectPath = FileBrowserHelpers.CreateFileInDirectory(parentDirectory, "effect.csv");
                FileBrowserHelpers.WriteTextToFile(effectPath, "id,timing,priority,target,condition,cond_option,effect,effect_option\n");    
            }
            FileBrowserHelpers.AppendTextToFile(effectPath, Effect.GetRawEffectListStringArray(info.id, info.effects).ConcatToString(",") + "\n");

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
            // Write info.
            string infoPath = skillPath + "info.csv";
            if (!FileBrowserHelpers.FileExists(infoPath)) {
                string parentDirectory = FileBrowserHelpers.GetDirectoryName(infoPath);
                infoPath = FileBrowserHelpers.CreateFileInDirectory(parentDirectory, "info.csv");
                FileBrowserHelpers.WriteTextToFile(infoPath, "id,name,element,type,power,anger,accuracy,option,description\n");    
            }
            FileBrowserHelpers.AppendTextToFile(infoPath, info.GetRawInfoStringArray().ConcatToString(",") + "\n");

            // Write effect.
            string effectPath = skillPath + "effect.csv";
            if (!FileBrowserHelpers.FileExists(effectPath)) {
                string parentDirectory = FileBrowserHelpers.GetDirectoryName(effectPath);
                effectPath = FileBrowserHelpers.CreateFileInDirectory(parentDirectory, "effect.csv");
                FileBrowserHelpers.WriteTextToFile(effectPath, "id,timing,priority,target,condition,cond_option,effect,effect_option\n");    
            }
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
}
