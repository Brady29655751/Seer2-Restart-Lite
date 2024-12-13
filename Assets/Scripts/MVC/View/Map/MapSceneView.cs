using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class MapSceneView : UIModule
{
    public ResourceManager RM => ResourceManager.instance;
    public Player player => Player.instance;
    [SerializeField] private RectTransform canvasRect;
    [SerializeField] private Image background, pathMask;

    private Map map;
    private Dictionary<int, NpcController> npcDict = new Dictionary<int, NpcController>();
    private Dictionary<int, GameObject> teleportDict = new Dictionary<int, GameObject>();

    public Vector2 GetCanvasSize() {
        return canvasRect.rect.size;
    }

    public void SetMap(Map map) {
        this.map = map;
        SetResources(map.resources);
        SetEntites(map.entities);
        CheckBattleResult();

        CheckDailyLogin();        
    }

    public void SetResources(MapResources resources) {
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
        background.sprite = resources.bg;
        background.color = map.dream ? Color.gray : map.backgroundColor;
    }

    public void SetPathMask(MapResources resources) {
        pathMask.sprite = resources.pathMaskSprite;
    }

    #endregion

    #region  entites

    public void SetEntites(MapEntities entities) {
        SetNpc(entities.npcs);
        SetTeleport(entities.teleports);
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

        if (player.currentNpcId == 0) {
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
            actionList = battleInfo.winHandler.Select(x => NpcHandler.GetNpcEntity(npc, x, npcDict)).ToList();

            /*
            // 主寵訓練營特殊活動
            if ((map.id == 60) && ((npc?.GetInfo()?.id ?? 0) >= 6000))
                actionList.AddRange(actionList);
            */
            
        } else if (result.isOpWin) {
            actionList = battleInfo.loseHandler.Select(x => NpcHandler.GetNpcEntity(npc, x, npcDict)).ToList();
        }

        foreach (var action in actionList) {
            action.Invoke();
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
