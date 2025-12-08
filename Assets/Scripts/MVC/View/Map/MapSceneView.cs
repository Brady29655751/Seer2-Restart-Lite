using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class MapSceneView : UIModule
{
    public ResourceManager RM => ResourceManager.instance;
    public Player player => Player.instance;
    [SerializeField] private RectTransform canvasRect;
    [SerializeField] private Image background, pathMask;

    private Map map;
    private Map lastMap => Player.instance.lastMap;
    private Dictionary<int, NpcController> npcDict = new Dictionary<int, NpcController>();
    private Dictionary<int, NpcController> farmDict = new Dictionary<int, NpcController>();
    private Dictionary<int, GameObject> teleportDict = new Dictionary<int, GameObject>();

    public Vector2 GetCanvasSize() {
        return canvasRect.rect.size;
    }

    public void SetMap(Map map) {
        this.map = map;
        bool refreshBGM = (SceneLoader.instance.GetLastScene() != SceneId.Map) || (lastMap == null) || (!map.music.ValueEquals(lastMap?.music));
        
        SetResources(map.resources, refreshBGM);
        SetEntites(map.entities);
        CheckBattleResult();

        CheckDailyLogin();        
    }

    public void SetResources(MapResources resources, bool refreshBGM = true) {
        if (refreshBGM)
            SetBGM(resources);
            
        SetBackground(resources);
        SetPathMask(resources);
    }

    #region resources

    public void SetBGM(MapResources resources) {
        AudioSystem.instance.PlayMusic(resources.bgm);
        AudioSystem.instance.PlayEffect(resources.fx);
    }

    public void SetBackground(MapResources resources) {
        var prefab = resources.anim;
        if (prefab == null)
        {
            background.sprite = resources.bg;
            background.color = map.dream ? Color.gray : map.backgroundColor;
            return;
        }

        var anim = Instantiate(prefab, Camera.main.transform);
        anim.transform.localScale = map.anim?.animScale ?? Vector2.zero;
        anim.transform.localPosition = map.anim?.animPos ?? Vector2.one;
        background.color = Color.clear;
    }

    public void SetPathMask(MapResources resources) {
        pathMask.sprite = resources.pathMaskSprite;
    }

    #endregion

    #region  entites

    public void SetEntites(MapEntities entities) {
        SetFarm(entities.farms);
        SetNpc(entities.npcs);
        SetTeleport(entities.teleports);

        if (!ListHelper.IsNullOrEmpty(entities.farms))
            StartCoroutine(CheckPlantRipeCoroutine());
    }

    public void SetNpc(List<NpcInfo> infos) {
        foreach (var npcInfo in infos) {
            var prefab = RM.GetPrefab("Map/Npc");
            GameObject obj = Instantiate(prefab, transform);
            NpcController npc = obj.GetComponent<NpcController>();
            NpcHandler.CreateNpc(npc, npcInfo, npcDict, infoPrompt);
        }
        Player.SetSceneData("mapNpcList", npcDict);

        foreach (var npcInfo in infos) {
            var autoActionList = npcInfo.eventHandler.Where(x => x.type == ButtonEventType.Auto)
                .Select(x => NpcHandler.GetNpcEntity(npcDict.Get(npcInfo.id), x, npcDict)).ToList();
            autoActionList.ForEach(x => x?.Invoke());
        }
    }

    public void SetFarm(List<NpcInfo> infos) {
        if (ListHelper.IsNullOrEmpty(infos))
            return;

        foreach (var npcInfo in infos) {
            var prefab = RM.GetPrefab("Map/Npc");
            GameObject obj = Instantiate(prefab, transform);
            NpcController npc = obj.GetComponent<NpcController>();
            NpcHandler.CreateFarm(npc, npcInfo, npcDict, infoPrompt);
            farmDict.Add(npcInfo.id, npc);
        }
    }

    private IEnumerator CheckPlantRipeCoroutine() {
        var activity = Activity.Find("farm");
        while (true) {
            foreach (var land in farmDict) {
                var id = land.Key;
                var npc = npcDict.Get(id + 100);
                var plant = activity.GetData("land[" + id + "].plant", "none");
                var isPlantGrowing = int.TryParse(plant, out var plantId);
                if (isPlantGrowing) {
                    var item = Item.GetItemInfo(plantId);
                    var date = DateTime.Parse(activity.GetData("land[" + id + "].date[ripe]", DateTime.MaxValue.ToString()));
                    var ripe = DateTime.Now >= date;
                    var icon = ripe ? plantId : int.Parse(item.options.Get("seed", "600000"));
                    npc.SetIcon((ItemInfo.IsMod(icon) ? "Mod/" : string.Empty) + "Items/" + icon);
                }
                npc.SetColor(isPlantGrowing ? Color.white : Color.clear);
            }
            yield return null;
        }
    }

    public void SetTeleport(List<TeleportInfo> infos) {
        foreach (var teleport in infos) {
            var prefab = RM.GetPrefab("Map/Teleport");
            GameObject obj = Instantiate(prefab, transform);
            TeleportHandler.CreateTeleport(teleportDict, obj, teleport, infoPrompt);
        }
        Player.SetSceneData("mapTeleportList", teleportDict);
    }

    #endregion

    public void CheckBattleResult() {
        if (player?.currentBattle?.info == null) {
            // player.currentBattle = null;
            return;
        }

        if ((player.currentNpcId == 0) || (player.currentNpcId.IsInRange(50000, 50100) && (player.currentMapId != 500))) {
            // player.currentBattle = null;
            return;
        }

        NpcController npc = npcDict.Get(Player.instance.currentNpcId);
        BattleInfo battleInfo = player.currentBattle.info;
        BattleResult result = player.currentBattle.result;
        Pet[] petBag = player.petBag;
        VersionPetData petData = GameManager.versionData.petData;
        List<Action> actionList = new List<Action>();

        if (result.isMyWin) { 
            actionList = battleInfo.winHandler?.Select(x => NpcHandler.GetNpcEntity(npc, x, npcDict)).ToList();

            // 1周年争霸赛特殊活動
            if ((map.id.IsInRange(82, 90)) && ((npc?.GetInfo()?.id ?? 0) >= 8200)) {
                var item = new Item(7, Random.Range(1, 5));
                Item.Add(item);
                Item.OpenHintbox(item);
            }
            
        } else if (result.isOpWin) {
            actionList = battleInfo.loseHandler?.Select(x => NpcHandler.GetNpcEntity(npc, x, npcDict)).ToList();
        }

        foreach (var action in actionList ?? new List<Action>()) {
            action?.Invoke();
        }

        Player.instance.currentNpcId = 0;
    }

    private void CheckDailyLogin() {
        var activity = Activity.Find("daily_login");
        if (bool.Parse(activity.GetData("news", "false")))
            return;

        Panel.OpenPanel<NewsPanel>();
        activity.SetData("news", "true");
        SaveSystem.SaveData();
    }
}
