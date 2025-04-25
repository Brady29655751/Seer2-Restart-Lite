using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    [SerializeField] public bool debugMode = true;
    [SerializeField] public bool onlineMode = false;
    public static string serverUrl => "Data/";
    public static string gameFilePostfix => (Application.platform == RuntimePlatform.Android) ? ".apk" : ".zip";
    public static string gameDownloadUrl => GetGameDownloadUrl();
    public static string resourceUrl => "https://www.dropbox.com/scl/fi/8skjwkfo4ujqal0b9jwnz/Basic.zip?rlkey=ips7c15koiamebdpu2lj3xk6p&st=9ueek3ay&dl=1";
    public static string petAnimationUrl => GetPetAnimationUrl();
    public static string versionDataRoute => GameManager.instance.onlineMode ? versionDataUrl : versionDataPath;
    private static string versionDataUrl => "https://www.dropbox.com/scl/fi/ibt7ibuvry3ppitkxtgld/version.xml?rlkey=fk5dd9ujdan5hmy6vvo5388r5&st=lzypn681&dl=1";
    private static string versionDataPath => serverUrl + "System/version.xml";
    public static VersionData versionData { get; private set; } = null;
    public static GameState state { get; private set; }
    public static event Action<GameState> OnBeforeStateChangedEvent;
    public static event Action<GameState> OnAfterStateChangedEvent;

    public static string GetGameDownloadUrl() {
        return (Application.platform == RuntimePlatform.Android) 
            ? "https://www.dropbox.com/scl/fi/8yt7abrdpjdtmjdvs9bh0/Seer2-Restart-Lite.Apk?rlkey=os61hdxoj9raqd7pydj4jgdsv&st=uwwvdk5b&dl=1"
            : "https://www.dropbox.com/scl/fi/2vvyvtkvzw6lah9frxe2y/Seer2-Restart-Lite.zip?rlkey=ru85f7y050ntgfj50eny9hxy0&st=rpe4u9pf&dl=1";
    }

    public static string GetPetAnimationUrl() {
        return (Application.platform == RuntimePlatform.Android) 
            ? "https://www.dropbox.com/scl/fi/7mll6bawk23ay3i6o6pe4/PetAnimationAndroid.zip?rlkey=19ejc4x3f1xg4cafsif95x3u9&st=zzuixt7y&dl=1" 
            : "https://www.dropbox.com/scl/fi/oeagun3npal6xks29z122/PetAnimationPC.zip?rlkey=1exlcxl4v776kr40dwfau8i1c&st=1b9t0tw9&dl=1";
    }

    protected void Start() {
        Utility.InitScreenSizeWithRatio(16, 9);
        ChangeState(GameState.Init);
    }

    public void ChangeState(GameState newState) {
        OnBeforeStateChangedEvent?.Invoke(newState);
        state = newState;
        
        Debug.Log($"New State: {newState}");

        switch(newState) {
            case GameState.Quit:
                GameQuit();
                break;
            case GameState.Init:
                GameInit();
                break;
            case GameState.Login:
                GameLogin();
                break;
            case GameState.Play:
                GamePlay();
                break;
            case GameState.Fight:
                GameFight();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(newState), newState, "Game Manager cannot change to unknown game state.");
        }
        OnAfterStateChangedEvent?.Invoke(newState);
    }

    private void GameInit() {
        GetVersionData();
    }

    private void GameQuit() {
        Application.Quit();
    }

    private void GameLogin() {
        GameData gameData = Player.instance.gameData;
        if (gameData.version != versionData.gameVersion) {
            Mail.VersionUpdate();
            Mission.VersionUpdate();
            Pet.VersionUpdate();
            BattleRecord.VersionUpdate();
            Activity.VersionUpdate();
        }
        gameData.version = versionData.gameVersion;

        if ((DateTime.Now.Date - gameData.lastLoginDate.Date).Days > 0) {
            Mail.DailyLogin();
            Mission.DailyLogin();
            Activity.DailyLogin();            
        }
        Player.instance.gameData.lastLoginDate = DateTime.Now;

        if (debugMode) {
            Player.instance.gameData.petStorage = Database.instance.petInfoDict.Select(entry => Pet.GetExamplePet(entry.Key)).ToList();
            Player.instance.gameData.itemStorage = Database.instance.itemInfoDict.Select(entry => new Item(entry.Key, 99999)).ToList();
            Player.instance.gameData.coin = (int)2e7;
            Player.instance.gameData.diamond = (int)2e7;
        }

        SaveSystem.SaveData();
        ChangeState(GameState.Play);
    }

    private void GamePlay() {}
    private void GameFight() {}

    public void GetVersionData(Action onFinish = null) {
        void OnRequestSuccess(VersionData data) {
            if (data == null) {
                OnRequestFail(null);
                return;
            }
            versionData = data;
            Database.instance.Init();
            onFinish?.Invoke();
        }
        void OnRequestFail(string error) {
            if (!onlineMode)
                OnLocalRequestFail(error);
            else {
                onlineMode = false;
                ResourceManager.LoadXML<VersionData>(versionDataPath, OnRequestSuccess, OnLocalRequestFail);
            }
        }
        void OnLocalRequestFail(string error) {
            versionData = new VersionData();
            RequestManager.OnRequestFail("获取本地版本档案失败，请重新启动游戏");
            onFinish?.Invoke();
        }
        GameManager.versionData = null; 
        ResourceManager.LoadXML<VersionData>(versionDataRoute, OnRequestSuccess, OnRequestFail);
    }
}

public enum GameState {
    Quit = -1,
    Init = 0,
    Login = 1,
    Play = 2,
    Fight = 3,
}
