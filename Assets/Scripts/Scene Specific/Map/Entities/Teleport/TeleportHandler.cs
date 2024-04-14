using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TeleportHandler
{
    public static ResourceManager RM => ResourceManager.instance;

    public static void CreateTeleport(Dictionary<int, GameObject> teleportList, GameObject teleport, TeleportInfo info, InfoPrompt infoPrompt) {
        SetTeleportRect(teleport, info);
        SetTeleportButton(teleport, info, infoPrompt);
        teleportList.Add(info.id, teleport);
    }

    private static void SetTeleportRect(GameObject teleport, TeleportInfo info) {
        RectTransform rect = teleport.GetComponent<RectTransform>();
        rect.SetAsLastSibling();
        rect.anchoredPosition = info.pos;
    }

    private static void SetTeleportButton(GameObject teleport, TeleportInfo info, InfoPrompt infoPrompt) {
        IButton button = teleport.GetComponent<IButton>();
        button.onPointerEnterEvent.SetListener(() => infoPrompt.SetActive(true));
        button.onPointerExitEvent.SetListener(() => infoPrompt.SetActive(false));
        button.onPointerOverEvent.SetListener(() => infoPrompt.SetInfoPromptWithAutoSize(info.name, TextAnchor.MiddleCenter));
        button.onPointerClickEvent.SetListener(() => Transport(info.pos, () => Teleport(info)));
    }

    public static void Transport(Vector2 canvasPos, Action onArrive) {
        PlayerController.instance.SetDestinationByCanvasPos(canvasPos, onArrive);
    }

    public static void Teleport(TeleportInfo info) {
        // Player.SetSceneData("mapInitPos", info.targetPos);
        Player.instance.currentMapId = info.targetMapId;
        SceneLoader.instance.ChangeScene(SceneId.Map);
    }

    public static void Teleport(int mapId, Vector2 targetPos = default(Vector2)) {
        if (mapId == 0) {
            Hintbox hintbox = Hintbox.OpenHintbox();
            hintbox.SetTitle("提示");
            hintbox.SetContent("地图暂未开放", 16, FontOption.Arial);
            hintbox.SetOptionNum(1);
            return;
        }

        if (targetPos != default(Vector2)) {
            Player.SetSceneData("mapInitPos", targetPos);
        }
        Player.instance.currentMapId = mapId;
        SceneLoader.instance.ChangeScene(SceneId.Map);
    }

    public static void SwitchDayNight(Vector2 targetPos = default(Vector2)) {
        Teleport(-Player.instance.currentMapId, targetPos);
    }
}
