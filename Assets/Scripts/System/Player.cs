using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Singleton<Player>
{
    public int gameDataId;
    public GameData gameData = new GameData();
    public string nickname => gameData.nickname;
    public Pet[] petBag {
        get => gameData.petBag;
        set => gameData.petBag = value;
    }
    public int petBagCursor = 0;
    public Pet pet => petBag[petBagCursor];
    
    public int currentMapId = 70;
    public Map currentMap = null;

    public int currentNpcId = 0;
    public int currentMissionId = 0;

    public Battle currentBattle = null;
    private static Dictionary<string, object> sceneData = new Dictionary<string, object>();

    private void Start() {
        gameDataId = -1;
    }

    protected override void OnApplicationQuit() {
        // if (gameDataId != -1) {
        //     SaveSystem.SaveData(gameData);
        // }
        base.OnApplicationQuit();
    }    

    public static object GetSceneData(string key, object defaultReturn = null) {
        return sceneData.Get(key, defaultReturn);
    }
    public static void SetSceneData(string key, object value) {
        sceneData.Set(key, value);
    }

    public static void RemoveSceneData(string key) {
        sceneData.Remove(key);
    }

}
