using System;
using System.IO;
using System.Xml.Serialization;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using UnityEngine.Networking;
using System.Threading.Tasks;
using UnityEngine.AddressableAssets;

public class ResourceManager : Singleton<ResourceManager>
{
    private string mapUrl => GameManager.serverUrl + "Maps/";
    private string petUrl => GameManager.serverUrl + "Pets/";
    private string skillUrl => GameManager.serverUrl + "Skills/";
    private string buffUrl => GameManager.serverUrl + "Buffs/";
    private string itemUrl => GameManager.serverUrl + "Items/";
    private string missionUrl => GameManager.serverUrl + "Missions/";
    private string activityUrl => GameManager.serverUrl + "Activities/";

    public string spritePath => "Sprites/";
    public string fontPath => "Fonts/";
    public string BGMPath => "BGM/";
    public string SEPath => "SE/";
    public string prefabPath => "Prefabs/";
    public string panelPath => "Panels/";
    public string mapPath => "Maps/";

    public string numString => "0123456789%";
    public string[] fontString => new string[]{"Arial", "MSJH", "Simsun", "Standard", "Weibei", "Mini Diamond", "Zongyi"};
    public Dictionary<string, Object> resDict = new Dictionary<string, Object>();

    // Start is called before the first frame update
    void Start()
    {
        Init();
    }

    // Initialize.
    private void Init() {
        InitUIResources();
        InitGameResources();
    }

    private void InitUIResources() {
        for (int i = 0; i < fontString.Length; i++) {
            LoadFont(fontString[i]);
        }
        for (int i = 0; i < numString.Length; i++) {
            LoadSprite("Numbers/Blue/" + numString[i].ToString());
        }
    }

    private void InitGameResources() {
        InitAll<Sprite>(spritePath + "Game/Elements", spritePath + "Elements");
        InitAll<Sprite>(spritePath + "Game/Genders", spritePath + "Genders");
        InitAll<Sprite>(spritePath + "Game/IV/Rank", spritePath + "IVRank");
        InitAll<Sprite>(spritePath + "Game/Personality/Relation", spritePath + "Personality/Relation");
        InitAll<Sprite>(spritePath + "Game/Emblems", spritePath + "Emblems");
        InitAll<Sprite>(spritePath + "Game/Skills", spritePath + "Skills");
    }

    public void UnloadModResources() {
        var resKeys = resDict.Keys.ToList();
        foreach (var key in resKeys) {
            if (!key.StartsWith("Mod/"))
                continue;

            resDict.Set(key, null);
        }
    }

    public void Set<T>(string resPath, T item) where T : Object {
        resDict.Set(resPath, item);
    }

    // Get. If not exists in resDict and loadWhenNull is true, load it.
    public T Get<T>(string item, bool loadWhenNull = true) where T : Object {
        T res = (T)resDict.Get(item);
        if (res != null)
            return res;

        if (!loadWhenNull)
            return res;
            
        return Load<T>(item);
    }
    public T GetLocalAddressables<T>(string item, bool isMod = false) where T : Object {
        if (isMod) {
            // Get cached resources first to prevent memory overhead.
            T cachedRes = Get<T>("Mod/" + item.TrimEnd(".png"), false);
            if (cachedRes != null)
                return cachedRes;

            var modPath = Application.persistentDataPath + "/Mod/" + item;

            // sprite only accepts png.
            if (typeof(T) == typeof(Sprite)) {
                modPath += (item.EndsWith(".png") ? string.Empty : ".png");
                if (!SaveSystem.TryLoadAllBytes(modPath, out var bytes))
                    return default(T);

                if (!SpriteSet.TryCreateSpriteFromBytes(bytes, out var sprite))
                    return default(T);

                Set<Sprite>("Mod/" + item.TrimEnd(".png"), sprite);
                return sprite as T;
            }
        }

        var dotIndex = item.IndexOf('.');
        if (dotIndex >= 0)
            item = item.Substring(0, dotIndex);

        return Get<T>("Addressables/" + item);
    }
    public Sprite GetSprite(string item) {
        Sprite s = (Sprite)resDict.Get(spritePath + item);
        return (s == null) ? LoadSprite(item) : s;
    }
    public Font GetFont(string item) {
        Font f = (Font)resDict.Get(fontPath + item);
        return (f == null) ? LoadFont(item) : f;
    }
    public Font GetFont(FontOption item) {
        string fontName = fontString[(int)item];
        Font f = (Font)resDict.Get(fontPath + fontName);
        return (f == null) ? LoadFont(fontName) : f;
    }
    public AudioClip GetSE(string item) {
        AudioClip clip = (AudioClip)resDict.Get(SEPath + item);
        return (clip == null) ? LoadSE(item) : clip;
    }
    public GameObject GetPrefab(string item) {
        GameObject prefab = (GameObject)resDict.Get(prefabPath + item);
        return (prefab == null) ? LoadPrefab(item) : prefab;
    }
    public GameObject GetPanel(string item) {
        GameObject panel = (GameObject)resDict.Get(panelPath + item + "/Panel");
        return (panel == null) ? LoadPanel(item) : panel;
    }


    // Load and Cache the resources in resDict.
    public T Load<T>(string path, string resPath = null) where T : Object {
        T res = Resources.Load<T>(path);
        resDict.Set((resPath == null) ? path : resPath, res);
        return res;
    }
    public Sprite LoadSprite(string path) {
        return Load<Sprite>(spritePath + path);
    }
    public Font LoadFont(string path) {
        return Load<Font>(fontPath + path);
    }
    public AudioClip LoadSE(string path) {
        return Load<AudioClip>(SEPath + path);
    }
    public GameObject LoadPrefab(string path) {
        return Load<GameObject>(prefabPath + path);
    }
    public GameObject LoadPanel(string path) {
        return Load<GameObject>(panelPath + path + "/Panel");
    }

    public static T GetXML<T>(string text) {
        if (text == null)
            return default(T);

        using (TextReader reader = new StringReader(text)) {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            T deserialized = (T)serializer.Deserialize(reader);
            reader.Close();
            return deserialized;
        };
    }

    public static T LoadXML<T>(string path, Action<T> onSuccess = null, Action<string> onFail = null) {
        if (path.StartsWith("http")) {
            void OnRequestSuccess(string text) => onSuccess?.Invoke(GetXML<T>(text));
            RequestManager.instance.Get(path, OnRequestSuccess, onFail);
            return default(T);
        }
        string text = Resources.Load<TextAsset>(path.TrimEnd(".xml"))?.text;
        if (text == null) {
            onFail?.Invoke(null);
            return default(T);
        }
        T xml = GetXML<T>(text);
        onSuccess?.Invoke(xml);
        return xml;
    }

    public static string[] GetCSV(string text) {
        if (text == null)
            return null;

        return text.Split(new char[]{',', '\n'}, System.StringSplitOptions.RemoveEmptyEntries);
    }

    public static string[] LoadCSV(string path, Action<string[]> onSuccess = null, Action<string> onFail = null) {
        if (path.StartsWith("http")) {
            void OnRequestSuccess(string text) => onSuccess?.Invoke(GetCSV(text));
            RequestManager.instance.Get(path, OnRequestSuccess, onFail);
            return null;
        }
        TextAsset textAsset = Resources.Load<TextAsset>(path.TrimEnd(".csv"));
        if (textAsset == null) {
            onFail?.Invoke(null);
            return null;
        }
        string[] csv = GetCSV(textAsset?.text);
        onSuccess?.Invoke(csv);
        return csv;
    }

    public void LoadMap(int id, Action<Map> onSuccess = null, Action<string> onFail = null) {
        void OnRequestSuccess(Map map) => LoadMapResources(map, onSuccess);
        if (id >= -50000)
            LoadXML<Map>(mapUrl + id + ".xml", OnRequestSuccess, error => onFail?.Invoke("地图加载失败，请重新启动游戏"));
        else
            SaveSystem.TryLoadMapMod(id, onSuccess, onFail);
    }

    private void LoadMapResources(Map map, Action<Map> onSuccess = null) {
        int resId = (map.resId == 0) ? map.id : map.resId;

        Sprite bg = GetLocalAddressables<Sprite>(mapPath + "bg/" + resId);
        Sprite pathSprite = GetLocalAddressables<Sprite>(mapPath + "path/" + Mathf.Abs(resId));
        AudioClip bgm = GetLocalAddressables<AudioClip>(BGMPath + map.music.category + "/" + map.music.bgm);
        AudioClip fx = string.IsNullOrEmpty(map.music.fx) ? null : GetLocalAddressables<AudioClip>(BGMPath + "fx/" + map.music.fx);
        MapResources mapResources = new MapResources(bg, pathSprite, bgm, fx);
        map.SetResources(mapResources);
        onSuccess?.Invoke(map);
    }

    // Note that one line in csv contains a list of effects, separated by \
    public List<List<Effect>> GetEffectLists(string[] data) {
        int dataCol = Effect.DATA_COL + 1;  // plus one for id.
        int dataRow = data.Length / dataCol;
        List<List<Effect>> effectLists = new List<List<Effect>>();
        for (int i = 1; i < dataRow; i++) {
            effectLists.Add(new List<Effect>());

            int cur = dataCol * i;
            if (!int.TryParse(data[cur], out var effectId)) {
                Debug.LogError("Effect id parsing failure.");
                return effectLists;
            }
            string[][] effectData = new string[dataCol - 1][];
            for (int j = 1; j < dataCol; j++) {
                effectData[j - 1] = data[cur+j].Split(new char[] {'\\'}, System.StringSplitOptions.RemoveEmptyEntries);
            }
            for (int j = 0; j < effectData[0].Length; j++) {
                Effect effect = new Effect(effectData[0][j], effectData[1][j], effectData[2][j], 
                    effectData[3][j], effectData[4][j], effectData[5][j], effectData[6][j]);
                effectLists[i-1].Add(effect);
            }
        }
        return effectLists;
    }

    public List<List<Effect>> LoadEffectLists(string path) {
        return GetEffectLists(LoadCSV(path));
    }

    public Dictionary<int, Skill> GetSkill(string[] info, string[] effects) {
        Dictionary<int, Skill> skillDict = new Dictionary<int, Skill>();

        var effectLists = GetEffectLists(effects);
        int dataCol = Skill.DATA_COL;
        int dataRow = info.Length / dataCol;
        Skill skill = null;
        for (int i = 1; i < dataRow; i++) {
            int cur = dataCol * i;
            skill = new Skill(info, cur);
            skill.SetEffects(effectLists[i-1]);
            skillDict.Add(skill.id, skill);
        }
        return skillDict;
    }

    public void LoadSkill(Action<Dictionary<int, Skill>> onSuccess = null) {
        LoadCSV(skillUrl + "info.csv", (data) => LoadSkillEffect(data, onSuccess));
    }

    private void LoadSkillEffect(string[] info, Action<Dictionary<int, Skill>> onSuccess = null, Action<string> onFail = null) {
        LoadCSV(skillUrl + "effect.csv", (data) => {
            var originalDict = GetSkill(info, data);
            if (SaveSystem.TryLoadSkillMod(out var modDict)) {
                foreach (var entry in modDict)
                    originalDict.Set(entry.Key, entry.Value);
            }
            onSuccess?.Invoke(originalDict);
        }, onFail);
    }

    public void LoadPetInfo(Action<Dictionary<int, PetInfo>> onSuccess = null) {
        StartCoroutine(GetPetInfo(onSuccess));
    }

    private IEnumerator GetPetInfo(Action<Dictionary<int, PetInfo>> onSuccess) {
        List<PetBasicInfo> basicInfo = null;
        Dictionary<int, PetFeatureInfo> featureInfo = null;
        Dictionary<int, PetExpInfo> expInfo = null;
        Dictionary<int, PetSkillInfo> skillInfo = null;
        Dictionary<int, PetUIInfo> uiInfo = null;
        Dictionary<int, PetInfo> petInfos = new Dictionary<int, PetInfo>();

        bool isFailed = false;
        LoadCSV(petUrl + "basic.csv", (data) => basicInfo = GetPetBasicInfo(data), (error) => isFailed = true);
        LoadCSV(petUrl + "feature.csv", (data) => featureInfo = GetPetFeatureInfo(data), (error) => isFailed = true);
        LoadCSV(petUrl + "exp.csv", (data) => expInfo = GetPetExpInfo(data), (error) => isFailed = true);
        LoadCSV(petUrl + "skill.csv", (data) => skillInfo = GetPetSkillInfo(data), (error) => isFailed = true);
        LoadCSV(petUrl + "ui.csv", (data) => uiInfo = GetPetUIInfo(data), (error) => isFailed = true);

        while ((basicInfo == null) || (featureInfo == null) || (expInfo == null) || (skillInfo == null) || (uiInfo == null)) {
            if (isFailed) {
                onSuccess?.Invoke(petInfos);
                yield break;
            }
            yield return null;
        }
        
        try {
            for (int i = 0; i < basicInfo.Count; i++) {
                var basic = basicInfo[i];
                var ui = uiInfo.Get(basic.id, new PetUIInfo(basic.id, basic.baseId));
                PetInfo info = new PetInfo(basic, featureInfo.Get(ui.defaultFeatureId), expInfo.Get(basic.id), new PetTalentInfo(), skillInfo.Get(basic.id), ui);
                petInfos.Set(info.id, info);
            }

            if (SaveSystem.TryLoadPetMod(out var modDict)) {
                foreach (var entry in modDict)
                    petInfos.Set(entry.Key, entry.Value);
            }
            onSuccess?.Invoke(petInfos);
        } catch (Exception) {
            onSuccess?.Invoke(new Dictionary<int, PetInfo>());
        }
    }

    public List<PetBasicInfo> GetPetBasicInfo(string[] data) {
        List<PetBasicInfo> petBasicInfos = new List<PetBasicInfo>();

        int dataCol = PetBasicInfo.DATA_COL;
        int dataRow = data.Length / dataCol;
        for (int i = 1; i < dataRow; i++) {
            int cur = dataCol * i;
            PetBasicInfo info = new PetBasicInfo(data, cur);
            petBasicInfos.Add(info);
        }
        return petBasicInfos;
    }

    public Dictionary<int, PetFeatureInfo> GetPetFeatureInfo(string[] data) {
        Dictionary<int, PetFeatureInfo> petFeatureInfos = new Dictionary<int, PetFeatureInfo>();

        int dataCol = PetFeatureInfo.DATA_COL;
        int dataRow = data.Length / dataCol;
        for (int i = 1; i < dataRow; i++) {
            int cur = dataCol * i;
            PetFeatureInfo info = new PetFeatureInfo(data, cur);
            petFeatureInfos.Set(info.baseId, info);
        }
        return petFeatureInfos;
    }

    public Dictionary<int, PetExpInfo> GetPetExpInfo(string[] data) {
        Dictionary<int, PetExpInfo> petExpInfos = new Dictionary<int, PetExpInfo>();

        int dataCol = PetExpInfo.DATA_COL;
        int dataRow = data.Length / dataCol;
        for (int i = 1; i < dataRow; i++) {
            int cur = dataCol * i;
            PetExpInfo info = new PetExpInfo(data, cur);
            petExpInfos.Set(info.id, info);
        }
        return petExpInfos;
    }

    public Dictionary<int, PetSkillInfo> GetPetSkillInfo(string[] data) {
        Dictionary<int, PetSkillInfo> petSkillInfos = new Dictionary<int, PetSkillInfo>();

        int dataCol = PetSkillInfo.DATA_COL;
        int dataRow = data.Length / dataCol;
        for (int i = 1; i < dataRow; i++) {
            int cur = dataCol * i;
            PetSkillInfo info = new PetSkillInfo(data, cur);
            petSkillInfos.Set(info.id, info);
        }
        return petSkillInfos;
    }

    public Dictionary<int, PetUIInfo> GetPetUIInfo(string[] data) {
        Dictionary<int, PetUIInfo> petUIInfos = new Dictionary<int, PetUIInfo>();

        int dataCol = PetUIInfo.DATA_COL;
        int dataRow = data.Length / dataCol;
        for (int i = 1; i < dataRow; i++) {
            int cur = dataCol * i;
            PetUIInfo info = new PetUIInfo(data, cur);
            petUIInfos.Set(info.id, info);
        }
        return petUIInfos;
    }

    public Dictionary<int, BuffInfo> GetBuffInfo(string[] data, string[] effects) {
        var buffInfoDict = new Dictionary<int, BuffInfo>();
        var effectLists = GetEffectLists(effects);
        int dataCol = BuffInfo.DATA_COL;
        int dataRow = data.Length / dataCol;
        for (int i = 1; i < dataRow; i++) {
            int cur = dataCol * i;
            BuffInfo info = new BuffInfo(data, cur);
            info.SetEffects(effectLists[i-1]);
            buffInfoDict.Set(info.id, info);
        }
        return buffInfoDict;
    }

    public void LoadBuffInfo(Action<Dictionary<int, BuffInfo>> onSuccess = null) {
        LoadCSV(buffUrl + "info.csv", (data) => LoadBuffEffect(data, onSuccess), (x) => {});
    }

    private void LoadBuffEffect(string[] info, Action<Dictionary<int, BuffInfo>> onSuccess = null) {
        LoadCSV(buffUrl + "effect.csv", (data) => {
            var originalDict = GetBuffInfo(info, data);
            if (SaveSystem.TryLoadBuffMod(out var modDict)) {
                foreach (var entry in modDict)
                    originalDict.Set(entry.Key, entry.Value);
            }
            onSuccess?.Invoke(originalDict);
        }, (x) => {});
    }

    public Dictionary<int, ItemInfo> GetItemInfo(string[] data, string[] effects) {
        Dictionary<int, ItemInfo> itemInfoDict = new Dictionary<int, ItemInfo>();
        var effectLists = GetEffectLists(effects);
        int dataCol = ItemInfo.DATA_COL;
        int dataRow = data.Length / dataCol;
        for (int i = 1; i < dataRow; i++) {
            int cur = dataCol * i;
            ItemInfo info = new ItemInfo(data, cur);
            info.SetEffects(effectLists[i - 1]);
            itemInfoDict.Set(info.id, info);
        }
        return itemInfoDict;
    }

    public void LoadItemInfo(Action<Dictionary<int, ItemInfo>> onSuccess = null) {
        LoadCSV(itemUrl + "info.csv", (data) => LoadItemEffect(data, onSuccess));
    }

    private void LoadItemEffect(string[] info, Action<Dictionary<int, ItemInfo>> onSuccess = null) {
        LoadCSV(itemUrl + "effect.csv", (data) => {
            var originalDict = GetItemInfo(info, data);
            if (SaveSystem.TryLoadItemMod(out var modDict)) {
                foreach (var entry in modDict)
                    originalDict.Set(entry.Key, entry.Value);
            }
            onSuccess?.Invoke(originalDict);
        });
    }

    public Dictionary<string, ActivityInfo> GetActivityInfo(string[] data) {
        Dictionary<string, ActivityInfo> activityInfoDict = new Dictionary<string, ActivityInfo>();
        int dataCol = ActivityInfo.DATA_COL;
        int dataRow = data.Length / dataCol;
        for (int i = 1; i < dataRow; i++) {
            int cur = dataCol * i;
            ActivityInfo info = new ActivityInfo(data, cur);
            activityInfoDict.Set(info.id, info);
        }
        return activityInfoDict;
    }

    public void LoadActivityInfo(Action<Dictionary<string, ActivityInfo>> onSuccess = null) {
        LoadCSV(activityUrl + "info.csv", (data) => {
            var originalDict = GetActivityInfo(data);
            if (SaveSystem.TryLoadActivityMod(out var modDict)) {
                foreach (var entry in modDict)
                    originalDict.Set(entry.Key, entry.Value);
            }
            onSuccess?.Invoke(originalDict);
        });
    }

    public void LoadMissionInfo(Action<Dictionary<int, MissionInfo>> onSuccess = null) {
        var missionData = GameManager.versionData.missionData;
        int loadedMission = 0;
        Dictionary<int, MissionInfo> missionInfoDict = new Dictionary<int, MissionInfo>();
        void OnRequestSuccess(int id, MissionInfo info) {
            loadedMission++;
            missionInfoDict.Set(id, info);
            if (loadedMission == missionData.totalMissionCount) {
                onSuccess?.Invoke(missionInfoDict);
            }
        }
        LoadMissionInfoDict(0, missionData.mainMissionCount, OnRequestSuccess);
        LoadMissionInfoDict(10000, missionData.sideMissionCount, OnRequestSuccess);
        LoadMissionInfoDict(20000, missionData.dailyMissionCount, OnRequestSuccess);
        LoadMissionInfoDict(30000, missionData.eventMissionCount, OnRequestSuccess);
    }

    private void LoadMissionInfoDict(int startIndex, int count, Action<int, MissionInfo> onSuccess) {
        for (int i = 1; i <= count; i++) {
            int id = startIndex + i;
            LoadXML<MissionInfo>(missionUrl + id + ".xml", (x) => onSuccess?.Invoke(id, x), (x) => {});
        }
    }

    private T[] InitAll<T>(string path, string key) where T : Object {
        T[] items = Resources.LoadAll<T>(path);
        items = items.OrderBy(x => int.Parse(x.name)).ToArray();
        for (int i = 0; i < items.Length; i++) {
            resDict.Set(key + "/" + i.ToString(), items[i]);
        }
        return items;
    }

}

public enum FontOption {
    Arial = 0,
    MSJH = 1,
    Simsun = 2,
    Standard = 3,
    Weibei = 4,
    MiniDiamond = 5,
    Zongyi = 6,
}
