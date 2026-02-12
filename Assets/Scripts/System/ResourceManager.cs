using System;
using System.IO;
using System.Xml.Serialization;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using System.Threading.Tasks;
using SimpleFileBrowser;

public class ResourceManager : Singleton<ResourceManager>
{
    private string mapUrl => GameManager.serverUrl + "Maps/";
    private string petUrl => GameManager.serverUrl + "Pets/";
    private string effectUrl => GameManager.serverUrl + "Effects/";
    private string skillUrl => GameManager.serverUrl + "Skills/";
    private string buffUrl => GameManager.serverUrl + "Buffs/";
    private string itemUrl => GameManager.serverUrl + "Items/";
    private string missionUrl => GameManager.serverUrl + "Missions/";
    private string activityUrl => GameManager.serverUrl + "Activities/";
    private string trpgUrl => GameManager.serverUrl + "TRPG/";

    public string spritePath => "Sprites/";
    public string fontPath => "Fonts/";
    public string BGMPath => "BGM/";
    public string SEPath => "SE/";
    public string prefabPath => "Prefabs/";
    public string panelPath => "Panels/";
    public string miniGamePath => "Games/";
    public string mapPath => "Maps/";
    public string bundlePath => "Bundles/";

    public string numString => "0123456789%";

    public string[] fontString => new string[]
        { "Arial", "MSJH", "Simsun", "Standard", "Weibei", "Mini Diamond", "Zongyi" };

    public Dictionary<string, Object> resDict = new Dictionary<string, Object>();
    private List<string> loadingAssetBundlesList = new List<string>();

    // Start is called before the first frame update
    void Start()
    {
        Init();
    }

    // Initialize.
    private void Init()
    {
        InitUIResources();
        InitGameResources();
        SaveSystem.TryLoadElementMod(out var error);
        Player.SetSceneData("is_mod_exist", SaveSystem.IsModExists() ? 1 : 0);
    }

    private void InitUIResources()
    {
        for (int i = 0; i < fontString.Length; i++)
        {
            LoadFont(fontString[i]);
        }

        for (int i = 0; i < numString.Length; i++)
        {
            LoadSprite("Numbers/Blue/" + numString[i].ToString());
        }
    }

    private void InitGameResources()
    {
        InitAll<Sprite>(spritePath + "Game/Elements", spritePath + "Elements");
        InitAll<Sprite>(spritePath + "Game/Genders", spritePath + "Genders");
        InitAll<Sprite>(spritePath + "Game/IV/Rank", spritePath + "IVRank");
        InitAll<Sprite>(spritePath + "Game/Personality/Relation", spritePath + "Personality/Relation");
        InitAll<Sprite>(spritePath + "Game/Emblems", spritePath + "Emblems");
        InitAll<Sprite>(spritePath + "Game/Skills", spritePath + "Skills");
    }

    public void UnloadResources()
    {
        var resKeys = resDict.Keys.ToList();
        foreach (var key in resKeys)
        {
            if (key.StartsWith("Mod/") || key.StartsWith("Resources/"))
                resDict.Set(key, null);
        }
    }

    public void Set<T>(string resPath, T item) where T : Object
    {
        resDict.Set(resPath, item);
    }

    // Get. If not exists in resDict and loadWhenNull is true, load it.
    public T Get<T>(string item, bool loadWhenNull = true) where T : Object
    {
        T res = (T)resDict.Get(item);
        if (res != null)
            return res;

        if (!loadWhenNull)
            return res;

        return Load<T>(item);
    }

    /// <summary>
    /// If T is Sprite, you can get the returned T immediately. <br/>
    /// If T is AudioClip, you should use "onSuccess" and "onFail". The immediate returned T will be null.
    /// </summary>
    public T GetLocalAddressables<T>(string item, bool isMod = false, Action<T> onSuccess = null, Action<string> onFail = null) where T : Object
    {
        var pathType = isMod ? "Mod/" : "Resources/";

        // Get cached resources first to prevent memory overhead.
        T cachedRes = Get<T>(pathType + item.TrimEnd(".png"), false);
        if (cachedRes != null) {
            onSuccess?.Invoke(cachedRes);
            return cachedRes;
        }

        var loadPath = Application.persistentDataPath + "/" + pathType + item;

        // sprite only accepts png.
        if (typeof(T) == typeof(Sprite))
        {
            loadPath += (item.EndsWith(".png") ? string.Empty : ".png");
            if (!SaveSystem.TryLoadAllBytes(loadPath, out var bytes)) {
                onFail?.Invoke("读取图片失败");
                return default(T);
            }

            if (!SpriteSet.TryCreateSpriteFromBytes(bytes, out var sprite)) {
                onFail?.Invoke("加载图片失败");
                return default(T);
            }

            Set<Sprite>(pathType + item.TrimEnd(".png"), sprite);

            cachedRes = sprite as T;
            onSuccess?.Invoke(cachedRes);
            return cachedRes;
        }

        if (typeof(T) == typeof(AudioClip)) 
        {
            loadPath += (item.EndsWith(".mp3") ? string.Empty : ".mp3");
            RequestManager.instance.DownloadAudioClip("file://" + loadPath, (clip) => onSuccess?.Invoke(clip as T), onFail);
            return default(T);
        }

        var dotIndex = item.IndexOf('.');
        if (dotIndex >= 0)
            item = item.Substring(0, dotIndex);

        cachedRes = Get<T>(item);
        onSuccess?.Invoke(cachedRes);
        return cachedRes;
    }

    public Sprite GetSprite(string item)
    {
        Sprite s = (Sprite)resDict.Get(spritePath + item);
        return (s == null) ? LoadSprite(item) : s;
    }

    public Font GetFont(string item)
    {
        Font f = (Font)resDict.Get(fontPath + item);
        return (f == null) ? LoadFont(item) : f;
    }

    public Font GetFont(FontOption item)
    {
        string fontName = fontString[(int)item];
        Font f = (Font)resDict.Get(fontPath + fontName);
        return (f == null) ? LoadFont(fontName) : f;
    }

    public AudioClip GetSE(string item)
    {
        AudioClip clip = (AudioClip)resDict.Get(SEPath + item);
        return (clip == null) ? LoadSE(item) : clip;
    }

    public GameObject GetPrefab(string item)
    {
        GameObject prefab = (GameObject)resDict.Get(prefabPath + item);
        return (prefab == null) ? LoadPrefab(item) : prefab;
    }

    public AssetBundle GetAssetBundle(string path, string resPath = null)
    {
        AssetBundle bundle = (AssetBundle)resDict.Get(resPath);
        return (bundle == null) ? LoadAssetBundle(path, resPath) : bundle;
    }

    public void GetAssetBundleAsync(string path, string resPath, Action<AssetBundle> onSuccess, Action<float> onProgress = null) {
        StartCoroutine(LoadAssetBundleAsync(path, resPath, onSuccess, onProgress));    
    }

    private IEnumerator CheckAnimIsDone(TaskCompletionSource<GameObject> task, dynamic animInfo)
    {
        while (animInfo.stuckTime <= 2f)
        {
            if (animInfo.isDone)
            {
                task.SetResult(animInfo.anim);
                yield break;
            }

            animInfo.stuckTime += 0.5f;
            yield return new WaitForSeconds(0.5f);
        }
    }

    public GameObject GetMapAnimPrefab(int mapID, string fileName)
    {
        try
        {
            var bundlePath = mapPath + "bundle/map_" + mapID;
            var folderPath = Map.IsMod(mapID) ? ("Mod/Maps/anim/" + GameManager.platform) : "PetAnimation/map";
            var mapAnimPath = folderPath + "/map_" + mapID;
            var prefabPath = mapPath + "anim/" + fileName;

            if (resDict.TryGetValue(prefabPath, out var value) && (value != null))
                return (GameObject)value;

            AssetBundle assetBundle = GetAssetBundle(mapAnimPath, bundlePath);
            if (assetBundle == null)
                return null;

            GameObject prefab = assetBundle.LoadAsset<GameObject>(fileName); //获取地图的某个特定动画
            if (prefab == null) //说明地图的某个特定动画没有
                return null;
        
            resDict.Set(prefabPath, prefab);
            return prefab;
        }
        catch (Exception)
        {
            return null;
        }
    }

    private readonly Dictionary<int, AssetBundle> petAssetBundleDictionary = new Dictionary<int, AssetBundle>();
    public AssetBundle GetPetAnimAssetBundle(int petID)
    {
        var bundlePath = "Pets/bundle/pfa_" + petID;

        try
        {
            var folderPath = PetInfo.IsMod(petID) ? ("Mod/Pets/anim/" + GameManager.platform) : "PetAnimation/pet";
            var petAnimPath = folderPath + "/pfa_" + petID;

            return GetAssetBundle(petAnimPath, bundlePath);
        }
        catch (Exception)
        {
            return null;
        }
    }

    public GameObject GetPetAnimPrefab(int petID, string fileName)
    {
        var prefabPath = "Pets/prefab/" + petID + "/" + fileName;

        if (resDict.TryGetValue(prefabPath, out var value) && (value != null))
        {
            return (GameObject)value;
        }

        try
        {
            AssetBundle assetBundle = GetPetAnimAssetBundle(petID);
            if (assetBundle == null)
                return null;

            var typeName = fileName.Substring(fileName.LastIndexOf('-') + 1);
            var allAssetNames = assetBundle.GetAllAssetNames().Select(x => x.Substring(x.LastIndexOf('/') + 1).TrimEnd(".prefab"));
            var assetName = allAssetNames.FirstOrDefault(x => x.Substring(x.LastIndexOf('-') + 1).ToLower() == typeName.ToLower());
            GameObject prefab = assetBundle.LoadAsset<GameObject>(assetName); //获取精灵的某个特定动画
            
            resDict.Set(prefabPath, prefab);

            return prefab;
        }
        catch (Exception)
        {
            return null;
        }
    }

    public GameObject GetPetAnimInstance(int petID, string fileName)
    {
        var animPath = "Pets/anim/" + fileName;

        try
        {
            var prefab = GetPetAnimPrefab(petID, fileName);
            if (prefab == null)
                return null;

            GameObject anim = Object.Instantiate(prefab);
            anim.transform.localScale = prefab.transform.localScale;
            anim.transform.position = prefab.transform.position;
            resDict.Set(animPath, anim);
            return anim;
        }
        catch (Exception)
        {
            return null;
        }
    }

    public void GetPetAnimAssetBundleAsync(int petID, Action<AssetBundle> onSuccess, Action<float> onProgress = null) {
        var bundlePath = "Pets/bundle/pfa_" + petID;
        var folderPath = PetInfo.IsMod(petID) ? ("Mod/Pets/anim/" + GameManager.platform) : "PetAnimation/pet";
        var petAnimPath = folderPath + "/pfa_" + petID;

        GetAssetBundleAsync(petAnimPath, bundlePath, onSuccess, onProgress);
    }

    private void GetPetAnimPrefabAsync(int petID, string fileName, Action<GameObject> onSuccess, Action<float> onProgress = null) {
        var prefabPath = "Pets/prefab/" + petID + "/" + fileName;

        if (resDict.TryGetValue(prefabPath, out var value) && (value != null))
        {
            onSuccess?.Invoke((GameObject)value);
            return;
        }

        GetPetAnimAssetBundleAsync(petID, assetBundle => 
        {
            if (assetBundle == null)
            {
                onSuccess?.Invoke(null);
                return;
            }

            var typeName = fileName.Substring(fileName.LastIndexOf('-') + 1);
            var allAssetNames = assetBundle.GetAllAssetNames().Select(x => x.Substring(x.LastIndexOf('/') + 1).TrimEnd(".prefab"));
            var assetName = allAssetNames.FirstOrDefault(x => x.Substring(x.LastIndexOf('-') + 1).ToLower() == typeName.ToLower());
            GameObject prefab = assetBundle.LoadAsset<GameObject>(fileName); //获取精灵的某个特定动画
            resDict.Set(prefabPath, prefab);
            onSuccess?.Invoke(prefab);
        }, onProgress);
    }

    public void GetPetAnimInstanceAsync(int petID, string fileName, Action<GameObject> onSuccess, Action<float> onProgress = null) {
        var animPath = "Pets/anim/" + fileName;

        if (resDict.TryGetValue(animPath, out var value) && (value != null))
        {
            onSuccess?.Invoke((GameObject)value);
            return;
        }

        GetPetAnimPrefabAsync(petID, fileName, prefab => {
            if (prefab == null)
            {
                onSuccess?.Invoke(null);
                return;
            }

            GameObject anim = Object.Instantiate(prefab);
            anim.transform.localScale = prefab.transform.localScale;
            anim.transform.position = prefab.transform.position;
            resDict.Set(animPath, anim);
            onSuccess?.Invoke(anim);
        }, onProgress);
    }

    public GameObject GetPanel(string item)
    {
        GameObject panel = (GameObject)resDict.Get(panelPath + item + "/Panel");
        return (panel == null) ? LoadPanel(item) : panel;
    }

    public GameObject GetMiniGame(string item)
    {
        GameObject panel = (GameObject)resDict.Get(miniGamePath + item + "/Game");
        return (panel == null) ? LoadMiniGame(item) : panel;
    }


    // Load and Cache the resources in resDict.
    public T Load<T>(string path, string resPath = null) where T : Object
    {
        T res = Resources.Load<T>(path);
        resDict.Set((resPath == null) ? path : resPath, res);
        return res;
    }

    public Sprite LoadSprite(string path)
    {
        return Load<Sprite>(spritePath + path);
    }

    public Font LoadFont(string path)
    {
        return Load<Font>(fontPath + path);
    }

    public AudioClip LoadSE(string path)
    {
        return Load<AudioClip>(SEPath + path);
    }

    public GameObject LoadPrefab(string path)
    {
        return Load<GameObject>(prefabPath + path);
    }

    public GameObject LoadPanel(string path)
    {
        return Load<GameObject>(panelPath + path + "/Panel");
    }

    public GameObject LoadMiniGame(string path)
    {
        return Load<GameObject>(miniGamePath + path + "/Game");
    }

    public AssetBundle LoadAssetBundle(string path, string resPath = null)
    {
        var bundlePath = resPath ?? (this.bundlePath + path);

        try
        {
            AssetBundle assetBundle = null;
            if (resDict.TryGetValue(bundlePath, out var value1) && (value1 != null))
            {
                assetBundle = (AssetBundle)value1; //缓存中有已经加载的AssetBundle,含有该精灵的所有动画
            }
            else
            {
                var filePath = Application.persistentDataPath + "/" + path;

                loadingAssetBundlesList.Add(filePath);

                if (FileBrowserHelpers.FileExists(filePath))
                    assetBundle = AssetBundle.LoadFromFile(filePath);

                loadingAssetBundlesList.Remove(filePath);
                resDict.Set(bundlePath, assetBundle);
            }

            return assetBundle;
        }
        catch (Exception)
        {
            return null;
        }
    }

    private IEnumerator LoadAssetBundleAsync(string path, string resPath, Action<AssetBundle> onSuccess, Action<float> onProgress = null)
    {
        var bundlePath = resPath ?? (this.bundlePath + path);
        AssetBundle assetBundle = null;
        if (resDict.TryGetValue(bundlePath, out var value1) && (value1 != null))
        {
            onSuccess?.Invoke((AssetBundle)value1);
            yield break;
        }

        var filePath = Application.persistentDataPath + "/" + path;
        AssetBundleCreateRequest request = null;
        try 
        {
            if (loadingAssetBundlesList.Contains(filePath))
            {
                onSuccess?.Invoke(null);
                yield break;
            }
            loadingAssetBundlesList.Add(filePath);
            if (FileBrowserHelpers.FileExists(filePath))
                request = AssetBundle.LoadFromFileAsync(filePath);
            else
                throw new FileNotFoundException();
        } 
        catch (Exception) 
        {
            loadingAssetBundlesList.Remove(filePath);
            onSuccess?.Invoke(null);
            yield break;
        }
        while (!request.isDone) {
            onProgress?.Invoke(request.progress);
            yield return null;
        }
        loadingAssetBundlesList.Remove(filePath);
        if ((assetBundle = request.assetBundle) == null)
        {
            onSuccess?.Invoke(null);
            yield break;
        }
        resDict.Set(bundlePath, assetBundle);
        onSuccess?.Invoke(assetBundle);
    }

    public static T GetXML<T>(string text)
    {
        if (text == null)
            return default(T);

        using (TextReader reader = new StringReader(text))
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            T deserialized = (T)serializer.Deserialize(reader);
            reader.Close();
            return deserialized;
        }
    }

    public static T LoadXML<T>(string path, Action<T> onSuccess = null, Action<string> onFail = null)
    {
        if (path.StartsWith("http"))
        {
            void OnRequestSuccess(string text) => onSuccess?.Invoke(GetXML<T>(text));
            RequestManager.instance.Get(path, OnRequestSuccess, onFail);
            return default(T);
        }

        string text = Resources.Load<TextAsset>(path.TrimEnd(".xml"))?.text;
        if (text == null)
        {
            onFail?.Invoke(null);
            return default(T);
        }

        T xml = GetXML<T>(text);
        onSuccess?.Invoke(xml);
        return xml;
    }

    public static string[] GetCSV(string text)
    {
        if (text == null)
            return null;

        return text.Split(new char[] { ',', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
    }

    public static string[] LoadCSV(string path, Action<string[]> onSuccess = null, Action<string> onFail = null)
    {
        if (path.StartsWith("http"))
        {
            void OnRequestSuccess(string text) => onSuccess?.Invoke(GetCSV(text));
            RequestManager.instance.Get(path, OnRequestSuccess, onFail);
            return null;
        }

        TextAsset textAsset = Resources.Load<TextAsset>(path.TrimEnd(".csv"));
        if (textAsset == null)
        {
            onFail?.Invoke(null);
            return null;
        }

        string[] csv = GetCSV(textAsset?.text);
        onSuccess?.Invoke(csv);
        return csv;
    }

    public void LoadMap(int id, Action<Map> onSuccess = null, Action<string> onFail = null)
    {
        if (!Map.IsMod(id))
            LoadXML<Map>(mapUrl + id + ".xml", (map) => LoadMapResources(map, onSuccess, onFail), error => onFail?.Invoke("加载地图失败，地图不存在"));
        else
            SaveSystem.TryLoadMapMod(id, onSuccess, onFail);
    }

    public void LoadMapResources(Map map, Action<Map> onSuccess = null, Action<string> onFail = null) {
        StartCoroutine(GetMapResources(map, onSuccess, onFail));
    }

    private IEnumerator GetMapResources(Map map, Action<Map> onSuccess = null, Action<string> onFail = null) {
        int resId = (map.resId == 0) ? map.id : map.resId;
        bool isMod = Map.IsMod(resId);
        int pathResId = isMod ? resId : ((map.pathId == 0) ? Mathf.Abs(resId) : map.pathId);

        int doneRequestCount = 0;

        Sprite bg = null, pathSprite = null;
        AudioClip bgm = null, fx = null;
        string error = string.Empty;

        void OnLoadBGMSuccess(AudioClip clip){ bgm = clip; doneRequestCount++; }
        void OnLoadBGMFail(string message){ error = "加载地图失败，BGM不存在"; doneRequestCount++; }

        // Audio only accepts mp3
        GetLocalAddressables<AudioClip>($"BGM/{map.music?.category}/{map.music?.bgm}.mp3", isMod, OnLoadBGMSuccess, 
            (message) => 
            { 
                if (resId == 0)
                    GetLocalAddressables<AudioClip>($"BGM/FirstPage.mp3", isMod, OnLoadBGMSuccess, OnLoadBGMFail);
                else
                    OnLoadBGMFail(message);
            });

        if (string.IsNullOrEmpty(map.music?.fx))
            doneRequestCount++;
        else {
            GetLocalAddressables<AudioClip>("BGM/fx/" + map.music.fx + ".mp3", isMod,
            (clip) => { fx = clip; doneRequestCount++; }, (message) => { error = "加载地图失败，FX不存在"; doneRequestCount++; });
        }   

        bg = GetLocalAddressables<Sprite>($"Maps/bg/{resId}", isMod);
        pathSprite = GetLocalAddressables<Sprite>("Maps/path/" + pathResId, isMod);

        if ((resId == 0) && (bg == null))
            bg = GetLocalAddressables<Sprite>("Activities/FirstPage", isMod);

        while (doneRequestCount < 2)
            yield return null;

        if (!string.IsNullOrEmpty(error) && (resId != 0)) {
            onFail?.Invoke(error);
            yield break;
        }

        var anim = GetMapAnimPrefab(resId, $"{resId}-idle");

        MapResources mapResources = new MapResources(bg, pathSprite, bgm, fx, anim);
        mapResources.anim = anim;
        map.SetResources(mapResources);
        onSuccess?.Invoke(map);
    }

    // Note that one line in csv contains a list of effects, separated by \
    public List<List<Effect>> GetEffectLists(string[] data)
    {
        int dataCol = Effect.DATA_COL + 1; // plus one for id.
        int dataRow = data.Length / dataCol;
        List<List<Effect>> effectLists = new List<List<Effect>>();
        for (int i = 1; i < dataRow; i++)
        {
            effectLists.Add(new List<Effect>());

            int cur = dataCol * i;
            if (!int.TryParse(data[cur], out var effectId))
            {
                Debug.LogError("Effect id parsing failure.");
                return effectLists;
            }
            try 
            {
                string[][] effectData = new string[dataCol - 1][];
                for (int j = 1; j < dataCol; j++)
                {
                    effectData[j - 1] =
                        data[cur + j].Split(new char[] { '\\' }, System.StringSplitOptions.RemoveEmptyEntries);
                }

                for (int j = 0; j < effectData[0].Length; j++)
                {
                    Effect effect = new Effect(effectData[0][j], effectData[1][j], effectData[2][j],
                        effectData[3][j], effectData[4][j], effectData[5][j], effectData[6][j]);

                    effect.id = effectId;
                    effectLists[i - 1].Add(effect);
                }
            } catch (Exception) {
                Hintbox.OpenHintboxWithContent("加载效果失败（ID：" + effectId + "）", 16);
            }
        }

        return effectLists;
    }

    public Dictionary<int, Effect> GetEffectDict(string[] data)
    {
        var effectDict = new Dictionary<int, Effect>();
        var effectList = GetEffectLists(data);
        
        for (int i = 0; i < effectList.Count; i++)
        {
            var e = effectList[i].FirstOrDefault();
            if (e == null)
                continue;
            effectDict.Add(e.id, e);
        }

        return effectDict;
    }

    public void LoadEffects(Action<Dictionary<int, Effect>> onSuccess = null)
    {
        LoadCSV(effectUrl + "effect.csv", (data) => {
            var originalDict = GetEffectDict(data);
            if (SaveSystem.TryLoadEffectMod(out var error, out var modDict))
            {
                foreach (var entry in modDict)
                    originalDict.Set(entry.Key, entry.Value);
            } else {
                if (error != string.Empty) {
                    var hintbox = Hintbox.OpenHintboxWithContent(error, 16);
                    hintbox.SetTitle("加载自制的预制效果失败");
                    hintbox.SetSize(720, 360);
                }
            }

            var LTE1W = originalDict.Where(x => x.Key.IsWithin(0, 10000)).ToList();
            foreach (var entry in LTE1W)
            {
                var e = new Effect(entry.Value);
                e.SetTiming(EffectTiming.None);
                originalDict.Set(entry.Key + 10000, e);    
            }

            onSuccess?.Invoke(originalDict);
        });
    }

    public Dictionary<int, Skill> GetSkill(string[] info, string[] effects, string[] sounds = null)
    {
        Dictionary<int, Skill> skillDict = new Dictionary<int, Skill>();

        var effectLists = GetEffectLists(effects);
        int dataCol = Skill.DATA_COL;
        int dataRow = info.Length / dataCol;
        Skill skill = null;
        try
        {
            for (int i = 1; i < dataRow; i++)
            {
                int cur = dataCol * i;
                skill = new Skill(info, cur);
                skill.SetEffects(effectLists[i - 1]);
                skillDict.Add(skill.id, skill);
            }
        }
        catch (Exception)
        {
            Hintbox.OpenHintboxWithContent("技能ID " + skill.id + "出错", 16);
        }

        return skillDict;
    }

    public void LoadSkill(Action<Dictionary<int, Skill>> onSuccess = null)
    {
        LoadCSV(skillUrl + "info.csv", (data) => LoadSkillEffect(data, onSuccess));
    }

    private void LoadSkillEffect(string[] info, Action<Dictionary<int, Skill>> onSuccess = null,
        Action<string> onFail = null)
    {
        LoadCSV(skillUrl + "effect.csv", (data) => {
            var originalDict = GetSkill(info, data);
            if (SaveSystem.TryLoadSkillMod(out var error, out var modDict))
            {
                foreach (var entry in modDict)
                    originalDict.Set(entry.Key, entry.Value);
            } else {
                if (error != string.Empty) {
                    var hintbox = Hintbox.OpenHintboxWithContent(error, 16);
                    hintbox.SetTitle("加载自制技能失败");
                    hintbox.SetSize(720, 360);
                }
            }
            onSuccess?.Invoke(originalDict);
        }, onFail);
    }

    public void LoadPetInfo(Action<Dictionary<int, PetInfo>> onSuccess = null,
        Action<Dictionary<int, PetFeatureInfo>> onFeatureSuccess = null,
        Action<Dictionary<int, PetHitInfo>> onHitSuccess = null,
        Action<Dictionary<int, PetSoundInfo>> onSoundSuccess = null)
    {
        StartCoroutine(GetPetInfo(onSuccess, onFeatureSuccess, onHitSuccess, onSoundSuccess));
    }

    private IEnumerator GetPetInfo(Action<Dictionary<int, PetInfo>> onSuccess,
        Action<Dictionary<int, PetFeatureInfo>> onFeatureSuccess,
        Action<Dictionary<int, PetHitInfo>> onHitSuccess = null,
        Action<Dictionary<int, PetSoundInfo>> onSoundSuccess = null)
    {
        List<PetBasicInfo> basicInfo = null;
        Dictionary<int, PetFeatureInfo> featureInfo = null;
        Dictionary<int, PetExpInfo> expInfo = null;
        Dictionary<int, PetSkillInfo> skillInfo = null;
        Dictionary<int, PetSkillInfo> cardInfo = null;
        Dictionary<int, PetUIInfo> uiInfo = null;
        Dictionary<int, PetHitInfo> hitInfo = new Dictionary<int, PetHitInfo>();
        Dictionary<int, PetSoundInfo> soundInfo = new Dictionary<int, PetSoundInfo>();
        Dictionary<int, PetInfo> petInfos = new Dictionary<int, PetInfo>();

        bool isFailed = false;
        LoadCSV(petUrl + "basic.csv", (data) => basicInfo = GetPetBasicInfo(data), (error) => isFailed = true);
        LoadCSV(petUrl + "feature.csv", (data) => featureInfo = GetPetFeatureInfo(data), (error) => isFailed = true);
        LoadCSV(petUrl + "exp.csv", (data) => expInfo = GetPetExpInfo(data), (error) => isFailed = true);
        LoadCSV(petUrl + "skill.csv", (data) => skillInfo = GetPetSkillInfo(data), (error) => isFailed = true);
        LoadCSV(petUrl + "card.csv", (data) => cardInfo = GetPetSkillInfo(data), (error) => isFailed = true);
        LoadCSV(petUrl + "ui.csv", (data) => uiInfo = GetPetUIInfo(data), (error) => isFailed = true);
        // LoadCSV(petUrl + "hit.csv", (data) => hitInfo = GetPetHitInfo(data), (error) => isFailed = true);
        // LoadCSV(petUrl + "sound.csv", (data) => soundInfo = GetPetSoundInfo(data), (error) => isFailed = true);

        var hitInfoPath = Application.persistentDataPath + "/PetAnimation/pet/hit.csv";
        var soundInfoPath = Application.persistentDataPath + "/PetAnimation/pet/sound.csv";

        if (FileBrowserHelpers.FileExists(hitInfoPath))
            hitInfo = GetPetHitInfo(GetCSV(FileBrowserHelpers.ReadTextFromFile(hitInfoPath)));

        if (FileBrowserHelpers.FileExists(soundInfoPath))
            soundInfo = GetPetSoundInfo(GetCSV(FileBrowserHelpers.ReadTextFromFile(soundInfoPath)));

        while ((basicInfo == null) || (featureInfo == null) || (expInfo == null) || (skillInfo == null) ||
            (cardInfo == null) || (uiInfo == null))
        {
            if (isFailed)
            {
                onSuccess?.Invoke(petInfos);
                yield break;
            }

            yield return null;
        }

        try
        {
            for (int i = 0; i < basicInfo.Count; i++)
            {
                var basic = basicInfo[i];
                var ui = uiInfo.Get(basic.id, new PetUIInfo(basic.id, basic.baseId));
                PetInfo info = new PetInfo(basic, featureInfo.Get(ui.defaultFeatureId), expInfo.Get(basic.id),
                    new PetTalentInfo(), skillInfo.Get(basic.id), ui);

                info.cards = cardInfo.Get(basic.id, skillInfo.Get(basic.id));
                petInfos.Set(info.id, info);
            }

            if (SaveSystem.TryLoadPetMod(out var error, out var modDict, out var featureDict, out var hitDict, out var soundDict))
            {
                foreach (var entry in modDict)
                    petInfos.Set(entry.Key, entry.Value);

                foreach (var entry in featureDict)
                    featureInfo.Set(entry.Key, entry.Value);

                foreach (var entry in hitDict)
                    hitInfo.Set(entry.Key, entry.Value);

                foreach (var entry in soundDict)
                    soundInfo.Set(entry.Key, entry.Value);
            }
            else
            {
                if (error != string.Empty)
                {
                    var hintbox = Hintbox.OpenHintboxWithContent(error, 16);
                    hintbox.SetTitle("加载自制精灵失败");
                    hintbox.SetSize(720, 360);
                }
            }

            onSuccess?.Invoke(petInfos);
            onFeatureSuccess?.Invoke(featureInfo);
            onHitSuccess?.Invoke(hitInfo);
            onSoundSuccess?.Invoke(soundInfo);
        }
        catch (Exception)
        {
            onSuccess?.Invoke(new Dictionary<int, PetInfo>());
            onFeatureSuccess?.Invoke(new Dictionary<int, PetFeatureInfo>());
            onHitSuccess?.Invoke(new Dictionary<int, PetHitInfo>());
            onSoundSuccess?.Invoke(new Dictionary<int, PetSoundInfo>());
        }
    }

    public List<PetBasicInfo> GetPetBasicInfo(string[] data)
    {
        List<PetBasicInfo> petBasicInfos = new List<PetBasicInfo>();

        int dataCol = PetBasicInfo.DATA_COL;
        int dataRow = data.Length / dataCol;
        for (int i = 1; i < dataRow; i++)
        {
            int cur = dataCol * i;
            if (data[cur].StartsWith("#"))
                continue;
                
            PetBasicInfo info = new PetBasicInfo(data, cur);
            petBasicInfos.Add(info);
        }

        return petBasicInfos;
    }

    public Dictionary<int, PetFeatureInfo> GetPetFeatureInfo(string[] data)
    {
        Dictionary<int, PetFeatureInfo> petFeatureInfos = new Dictionary<int, PetFeatureInfo>();

        int dataCol = PetFeatureInfo.DATA_COL;
        int dataRow = data.Length / dataCol;
        for (int i = 1; i < dataRow; i++)
        {
            int cur = dataCol * i;
            PetFeatureInfo info = new PetFeatureInfo(data, cur);
            petFeatureInfos.Set(info.baseId, info);
        }

        return petFeatureInfos;
    }

    public Dictionary<int, PetExpInfo> GetPetExpInfo(string[] data)
    {
        Dictionary<int, PetExpInfo> petExpInfos = new Dictionary<int, PetExpInfo>();

        int dataCol = PetExpInfo.DATA_COL;
        int dataRow = data.Length / dataCol;
        for (int i = 1; i < dataRow; i++)
        {
            int cur = dataCol * i;
            PetExpInfo info = new PetExpInfo(data, cur);
            petExpInfos.Set(info.id, info);
        }

        return petExpInfos;
    }

    public Dictionary<int, PetSkillInfo> GetPetSkillInfo(string[] data)
    {
        Dictionary<int, PetSkillInfo> petSkillInfos = new Dictionary<int, PetSkillInfo>();

        int dataCol = PetSkillInfo.DATA_COL;
        int dataRow = data.Length / dataCol;
        for (int i = 1; i < dataRow; i++)
        {
            int cur = dataCol * i;
            PetSkillInfo info = new PetSkillInfo(data, cur);
            petSkillInfos.Set(info.id, info);
        }

        return petSkillInfos;
    }

    public Dictionary<int, PetUIInfo> GetPetUIInfo(string[] data)
    {
        Dictionary<int, PetUIInfo> petUIInfos = new Dictionary<int, PetUIInfo>();

        int dataCol = PetUIInfo.DATA_COL;
        int dataRow = data.Length / dataCol;
        for (int i = 1; i < dataRow; i++)
        {
            int cur = dataCol * i;
            PetUIInfo info = new PetUIInfo(data, cur);
            petUIInfos.Set(info.id, info);
        }

        return petUIInfos;
    }

    public Dictionary<int, PetHitInfo> GetPetHitInfo(string[] data)
    {
        Dictionary<int, PetHitInfo> petHitInfos = new Dictionary<int, PetHitInfo>();
        if (data == null)
            return petHitInfos;

        int dataCol = PetHitInfo.DATA_COL;
        int dataRow = data.Length / dataCol;
        for (int i = 1; i < dataRow; i++)
        {
            int cur = dataCol * i;
            PetHitInfo info = new PetHitInfo(data, cur);
            petHitInfos.Set(info.skinId, info);
        }

        return petHitInfos;
    }

    public Dictionary<int, PetSoundInfo> GetPetSoundInfo(string[] data)
    {
        Dictionary<int, PetSoundInfo> petSoundInfos = new Dictionary<int, PetSoundInfo>();
        if (data == null)
            return petSoundInfos;

        int dataCol = PetSoundInfo.DATA_COL;
        int dataRow = data.Length / dataCol;
        for (int i = 1; i < dataRow; i++)
        {
            int cur = dataCol * i;
            PetSoundInfo info = new PetSoundInfo(data, cur);
            petSoundInfos.Set(info.skinId, info);
        }

        return petSoundInfos;
    }

    public Dictionary<int, BuffInfo> GetBuffInfo(string[] data, string[] effects)
    {
        var buffInfoDict = new Dictionary<int, BuffInfo>();
        var effectLists = GetEffectLists(effects);
        int dataCol = BuffInfo.DATA_COL;
        int dataRow = data.Length / dataCol;
        BuffInfo info = null;

        try
        {
            for (int i = 1; i < dataRow; i++)
            {
                int cur = dataCol * i;
                info = new BuffInfo(data, cur);
                info.SetEffects(effectLists[i - 1]);
                buffInfoDict.Set(info.id, info);
            }
        }
        catch (Exception)
        {
            Hintbox.OpenHintboxWithContent("印记ID " + info.id + "出错", 16);
        }

        return buffInfoDict;
    }

    public void LoadBuffInfo(Action<Dictionary<int, BuffInfo>> onSuccess = null)
    {
        LoadCSV(buffUrl + "info.csv", (data) => LoadBuffEffect(data, onSuccess), (x) => { });
    }

    private void LoadBuffEffect(string[] info, Action<Dictionary<int, BuffInfo>> onSuccess = null)
    {
        LoadCSV(buffUrl + "effect.csv", (data) =>
        {
            var originalDict = GetBuffInfo(info, data);
            if (SaveSystem.TryLoadBuffMod(out var error, out var modDict))
            {
                foreach (var entry in modDict)
                    originalDict.Set(entry.Key, entry.Value);
            } else {
                if (error != string.Empty) {
                    var hintbox = Hintbox.OpenHintboxWithContent(error, 16);
                    hintbox.SetTitle("加载自制印记失败");
                    hintbox.SetSize(720, 360);
                }
            }

            onSuccess?.Invoke(originalDict);
        }, (x) => { });
    }

    public Dictionary<int, ItemInfo> GetItemInfo(string[] data, string[] effects)
    {
        Dictionary<int, ItemInfo> itemInfoDict = new Dictionary<int, ItemInfo>();
        var effectLists = GetEffectLists(effects);
        int dataCol = ItemInfo.DATA_COL;
        int dataRow = data.Length / dataCol;
        ItemInfo info = null;
        try
        {
            for (int i = 1; i < dataRow; i++)
            {
                int cur = dataCol * i;
                info = new ItemInfo(data, cur);
                info.SetEffects(effectLists[i - 1]);
                itemInfoDict.Set(info.id, info);
            }
        }
        catch (Exception)
        {
            Hintbox.OpenHintboxWithContent("道具ID " + info.id + "出错", 16);
        }

        return itemInfoDict;
    }

    public void LoadItemInfo(Action<Dictionary<int, ItemInfo>> onSuccess = null)
    {
        LoadCSV(itemUrl + "info.csv", (data) => LoadItemEffect(data, onSuccess));
    }

    private void LoadItemEffect(string[] info, Action<Dictionary<int, ItemInfo>> onSuccess = null)
    {
        LoadCSV(itemUrl + "effect.csv", (data) =>
        {
            var originalDict = GetItemInfo(info, data);
            if (SaveSystem.TryLoadItemMod(out var error, out var modDict))
            {
                foreach (var entry in modDict)
                    originalDict.Set(entry.Key, entry.Value);
            } else {
                if (error != string.Empty) {
                    var hintbox = Hintbox.OpenHintboxWithContent(error, 16);
                    hintbox.SetTitle("加载自制道具失败");
                    hintbox.SetSize(720, 360);
                }
            }

            var skillData = GameManager.versionData.skillData;
            for (int id = skillData.minSkillId; id <= skillData.maxSkillId; id++) {
                if (originalDict.TryGetValue(10_0000 + id, out _))
                    continue;

                var skill = Skill.GetSkill(id, false);
                if (skill == null)
                    continue;

                var itemDescription = $"记载着技能学习诀窍的神秘书卷[ENDL]使精灵习得{skill.name}（对已习得此技能的精灵无效）";
                var effectDescription = Skill.GetSkillDescriptionPreview(skill.rawDescription);
                var skillItemInfo = new ItemInfo(10_0000 + id, "技能学习书（" + skill.name + "）", ItemType.Skill,
                    skill.isSuper ? 20 : 10, 6, "resId=110050", itemDescription, effectDescription);
                var effect = new Effect("resident", "-1", "pet", "pet", $"type=skill[{id}]&skill[{id}]_cmp=0", "set_pet", $"type=skill&op=+&value={id}");
                skillItemInfo.SetEffects(new List<Effect>(){ effect });
                originalDict[10_0000 + id] = skillItemInfo;
            }

            foreach (var buffInfo in BuffInfo.database.Where(x => x?.options?.Get("group") == "trait"))
            {
                if (buffInfo == null)
                    continue;

                var itemDescription = $"一位神秘发明家的研究成果，临床实验证明可以引导精灵体内的奇异能力。[ENDL]使精灵异能指定改变为{buffInfo.name}";
                var effectDescription = Buff.GetBuffDescriptionPreview(buffInfo.description);
                var buffItemInfo = new ItemInfo(10_0000 + buffInfo.id, "异能催化剂（" + buffInfo.name + "）", ItemType.Personality,
                    30, 6, $"resId=Buffs/{buffInfo.resId}", itemDescription, effectDescription);
                var effect = new Effect("resident", "-1", "pet", "none", "none", "set_pet", $"type=trait&op=SET&value={buffInfo.id}");
                buffItemInfo.SetEffects(new List<Effect>(){ effect });
                originalDict[10_0000 + buffInfo.id] = buffItemInfo;
            }

            onSuccess?.Invoke(originalDict);
        });
    }

    public Dictionary<string, ActivityInfo> GetActivityInfo(string[] data)
    {
        Dictionary<string, ActivityInfo> activityInfoDict = new Dictionary<string, ActivityInfo>();
        int dataCol = ActivityInfo.DATA_COL;
        int dataRow = data.Length / dataCol;
        for (int i = 1; i < dataRow; i++)
        {
            int cur = dataCol * i;
            ActivityInfo info = new ActivityInfo(data, cur);
            activityInfoDict.Set(info.id, info);
        }

        return activityInfoDict;
    }

    public void LoadActivityInfo(Action<Dictionary<string, ActivityInfo>> onSuccess = null)
    {
        LoadCSV(activityUrl + "info.csv", (data) =>
        {
            var originalDict = GetActivityInfo(data);
            if (SaveSystem.TryLoadActivityMod(out var error, out var modDict))
            {
                foreach (var entry in modDict)
                    originalDict.Set(entry.Key, entry.Value);
            } else {
                if (error != string.Empty) {
                    var hintbox = Hintbox.OpenHintboxWithContent(error, 16);
                    hintbox.SetTitle("加载自制活动失败");
                    hintbox.SetSize(720, 360);
                }
            }

            onSuccess?.Invoke(originalDict);
        });
    }

    public bool GetElementInfo(string[] data)
    {
        int dataCol = (int)Mathf.Sqrt(data.Length);
        int dataRow = data.Length / dataCol;

        PetElementSystem.elementList = PetElementSystem.elementList.Take(dataCol - 1).ToList();
        PetElementSystem.elementNameList = data.Skip(1).Take(dataCol - 1).ToList();
        PetElementSystem.elementDefenseRelation.Clear();

        for (int i = 1; i < dataRow; i++)
        {
            int cur = dataCol * i;
            PetElementSystem.elementDefenseRelation.Add((Element)(i - 1),
                data.GetRange(cur + 1, cur + dataCol).Select(float.Parse).ToList());
        }

        PetElementSystem.isMod = true;

        return PetElementSystem.IsMod();
    }

    public void LoadMissionInfo(Action<Dictionary<int, MissionInfo>> onSuccess = null)
    {
        var missionData = GameManager.versionData.missionData;
        int loadedMission = 0;
        Dictionary<int, MissionInfo> missionInfoDict = new Dictionary<int, MissionInfo>();

        void OnRequestSuccess(int id, MissionInfo info)
        {
            loadedMission++;
            missionInfoDict.Set(id, info);
            if (loadedMission == missionData.totalMissionCount)
            {
                onSuccess?.Invoke(missionInfoDict);
            }
        }

        LoadMissionInfoDict(0, missionData.mainMissionCount, OnRequestSuccess);
        LoadMissionInfoDict(10000, missionData.sideMissionCount, OnRequestSuccess);
        LoadMissionInfoDict(20000, missionData.dailyMissionCount, OnRequestSuccess);
        LoadMissionInfoDict(30000, missionData.eventMissionCount, OnRequestSuccess);
    }

    private void LoadMissionInfoDict(int startIndex, int count, Action<int, MissionInfo> onSuccess)
    {
        for (int i = 1; i <= count; i++)
        {
            int id = startIndex + i;
            LoadXML<MissionInfo>(missionUrl + id + ".xml", (x) => onSuccess?.Invoke(id, x), (x) => { });
        }
    }

    private T[] InitAll<T>(string path, string key) where T : Object
    {
        T[] items = Resources.LoadAll<T>(path);
        items = items.OrderBy(x => int.Parse(x.name)).ToArray();
        for (int i = 0; i < items.Length; i++)
        {
            resDict.Set(key + "/" + i.ToString(), items[i]);
        }

        return items;
    }
}

public enum FontOption
{
    Arial = 0,
    MSJH = 1,
    Simsun = 2,
    Standard = 3,
    Weibei = 4,
    MiniDiamond = 5,
    Zongyi = 6,
}