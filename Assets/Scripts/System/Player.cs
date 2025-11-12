using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

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

    public int currentMapId = 70, lastMapId = 0;
    public Map currentMap = null, lastMap = null;

    public int currentNpcId = 0;
    public int currentMissionId = 0;
    public Activity currentMiniGame = null;

    public int random
    {
        get => (int)GetSceneData("random", 0) / (petBag.Any(x => (x != null) && ((Buff.GetEmblemBuff(x)?.id ?? 0) == 20_0051)) ? 4 : 1);
        set => SetSceneData("random", value);
    }
    public Battle currentBattle = null;
    public BattleRecord currentBattleRecord = null;
    private static Dictionary<string, object> sceneData = new Dictionary<string, object>();

    public bool isShootMode {
        get => (bool)GetSceneData("shootMode", false);
        set => SetShootMode(value);
    }

    private void Start() {
        gameDataId = -1;
        random = Random.Range(0, 100);
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

    public void SetShootMode(bool shootMode) {
        if ((currentMapId == 990) && shootMode)
            return;
            
        var texture = shootMode ? SpriteSet.Aim : null;
        var hotspot = shootMode ? (texture.GetTextureSize() / 2) : Vector2.zero;
        Cursor.SetCursor(texture, hotspot, CursorMode.Auto);
        SetSceneData("shootMode", shootMode);
    }

}
