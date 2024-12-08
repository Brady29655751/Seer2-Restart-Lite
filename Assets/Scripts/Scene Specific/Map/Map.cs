using System;
using System.Linq;
using System.Xml.Serialization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[XmlRoot("map")]
public class Map
{
    [XmlAttribute("id")] public int id;
    [XmlAttribute("resId")] public int resId = 0;
    [XmlAttribute("pathId")] public int pathId = 0;
    [XmlAttribute("name")] public string name;
    [XmlAttribute("weather")] public int weather = 0;
    [XmlAttribute("color")] public string backgroundColorId = "255,255,255,255";
    public Color backgroundColor => backgroundColorId.ToColor(Color.white);
    
    [XmlAttribute("category")] public int categoryId;
    public MapCategory category => (MapCategory)categoryId;
    [XmlAttribute("switch")] public int dayNightSwitch;
    [XmlAttribute("dream")] public bool dream;

    [XmlElement("fightMap")] public int fightMapId;
    [XmlElement("fightMapColor")] public string fightMapColorId = "255,255,255,255";
    public Color fightMapColor => IsMod(id) ? fightMapColorId.ToColor(Color.white) : new Color(0.5f, 0.5f, 0.5f, 1);
    [XmlElement("initialPoint")] public string initialPoint;
    public Vector2 initPoint => initialPoint.ToVector2();
    
    //------------
    [XmlElement("music")] public MapMusic music;
    [XmlElement("entities")] public MapEntities entities;
    [XmlIgnore] public MapResources resources;
    [XmlIgnore] public Vector2Int pathSize => resources.pathSize;

    public static bool IsMod(int id) => id < -50000;
    public static void GetMap(int id, Action<Map> onSuccess = null, Action<string> onFail = null) {
       ResourceManager.instance.LoadMap(id, onSuccess, onFail);
    }

    public static void TestBattle(int mapId, int npcId, string battleId, Pet[] petBag = null) {
        Map.GetMap(mapId, (map) => Map.OnLoadTestBattleMapSuccess(map, npcId, battleId, petBag), 
            (error) => Hintbox.OpenHintboxWithContent(error, 16));
    }

    public static void TestBattle(int mapId, Pet[] petBag) {
        Map.GetMap(mapId, (map) => OnLoadTestBattleMapSuccess(map, mapId * 100 + (mapId > 0 ? 1 : -1), "test", petBag),
            (error) => Hintbox.OpenHintboxWithContent(error, 16));
    }

    private static void OnLoadTestBattleMapSuccess(Map map, int npcId, string battleId, Pet[] petBag = null) {
        if (map == null) {
            Hintbox.OpenHintboxWithContent("加载的地图为空", 16);
            return;
        }
        var battleInfo = map?.entities?.npcs?.Find(x => x.id == npcId)?.battleHandler?.Find(x => x?.id == battleId);
        if (battleInfo == null) {
            Hintbox.OpenHintboxWithContent("测试的NPC战斗信息为空", 16);
            return;
        }

        var player = ((petBag?.Select(BattlePet.GetBattlePet)) ?? (battleInfo.playerInfo?.Select(BattlePet.GetBattlePet))).ToArray();
        var enemy = battleInfo.enemyInfo?.Select(BattlePet.GetBattlePet).ToArray();

        if ((player == null) || (enemy == null)) {
            Hintbox.OpenHintboxWithContent("测试的精灵信息为空", 16);
            return;
        }
        BattleSettings settings = new BattleSettings(battleInfo.settings){ 
            isSimulate = true, 
            isCaptureOK = false 
        };
        Battle battle = new Battle(player, enemy, settings);
        SceneLoader.instance.ChangeScene(SceneId.Battle);
    }

    public void SetResources(MapResources resources) {
        this.resources = resources;
    }

    public bool IsPathAvailableByMousePos(Vector2 mousePos) {
        Vector2Int pathPixel = GetMapPixelByMousePos(mousePos);
        return IsPathAvailable(pathPixel);
    }

    public bool IsPathAvailable(Vector2Int pathPixel) {
        return IsPathAvailable(pathPixel.x, pathPixel.y);
    }
    public bool IsPathAvailable(int x, int y) {
        if (resources.pathTexture == null)
            return false;
        return resources.pathTexture.GetPixel(x, y) == Color.white;
    }

    public Vector2Int GetMapPixelByMousePos(Vector2 mousePos) {
        return mousePos.GetCorrespondingPixel(Utility.GetScreenSize(), pathSize);
    }

    public Vector2Int GetMousePixelByMapPos(Vector2 mapPos) {
        return mapPos.GetCorrespondingPixel(pathSize, Utility.GetScreenSize());
    }

    public Vector2Int GetCanvasPixelByMousePos(Vector2 mousePos, Vector2 canvasSize) {
        float canvasScaleFactor = Utility.GetScreenSize().x / canvasSize.x;
        return (mousePos / canvasScaleFactor).RoundToInt();
    }

    /// <summary>
    /// <paramref name="anchor"/> Bottom-left (0, 0). Top-right (1, 1).
    /// </summary>
    public Vector2Int GetCanvasPixelByMapPos(Vector2 mapPos, Vector2 canvasSize, Vector2 anchor = default(Vector2)) {
        Vector2 anchoredMapPos = mapPos.Anchor(pathSize, anchor);
        Vector2 mousePos = GetMousePixelByMapPos(anchoredMapPos);
        return GetCanvasPixelByMousePos(mousePos, canvasSize);
    }

}

public enum MapCategory {
    Ship = 1,
    Grass = 2,
    Station = 100,
}

