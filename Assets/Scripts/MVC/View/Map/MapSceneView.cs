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
        pathMask.sprite = resources.pathSprite;
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

        // Use mod pet to battle does not count. Unless mod battle
        if ((!petBag.Any(x => PetInfo.IsMod(x?.id ?? 0))) || (battleInfo.settings.isMod)) {
            if (result.isMyWin) { 
                actionList = battleInfo.winHandler.Select(x => NpcHandler.GetNpcEntity(npc, x, npcDict)).ToList();
            } else if (result.isOpWin) {
                actionList = battleInfo.loseHandler.Select(x => NpcHandler.GetNpcEntity(npc, x, npcDict)).ToList();
            }
        }

        foreach (var action in actionList) {
            action.Invoke();
        }

        Player.instance.currentNpcId = 0;
    }
}
