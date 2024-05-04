using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    [SerializeField] public bool debugMode = true;
    [SerializeField] public bool heliosMode = true;
    public static string serverUrl => "Data/";
    public static string versionDataUrl => serverUrl + "System/version.xml";
    public static string gameDownloadUrl => serverUrl + "Release/Seer2_Restart_Lite.zip";
    public static VersionData versionData { get; private set; } = null;
    public static GameState state { get; private set; }
    public static event Action<GameState> OnBeforeStateChangedEvent;
    public static event Action<GameState> OnAfterStateChangedEvent;

    protected void Start() {
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
        void OnRequestSuccess(VersionData data) {
            if (data == null) {
                OnRequestFail(null);
                return;
            }
            versionData = data;
            Database.instance.Init();
        }
        void OnRequestFail(string error) {
            versionData = new VersionData();
            RequestManager.OnRequestFail("获取版本档案失败，请重新启动游戏");
        } 
        
        Utility.InitScreenSizeWithRatio(16, 9);
        ResourceManager.LoadXML<VersionData>(versionDataUrl, OnRequestSuccess, OnRequestFail);
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
            gameData.version = versionData.gameVersion;
        }
        if ((DateTime.Now.Date - gameData.lastLoginDate.Date).Days > 0) {
            Mail.DailyLogin();
            Mission.DailyLogin();
            Activity.DailyLogin();            
        }
        Player.instance.gameData.lastLoginDate = DateTime.Now;

        if (debugMode) {
            Player.instance.gameData.petStorage = versionData.petData.petDictionary;
        }

        SaveSystem.SaveData();
        ChangeState(GameState.Play);
    }

    private void GamePlay() {}
    private void GameFight() {}
}

public enum GameState {
    Quit = -1,
    Init = 0,
    Login = 1,
    Play = 2,
    Fight = 3,
}
