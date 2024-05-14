using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapBlockView : Module
{
    [SerializeField] protected InfoPrompt infoPrompt;
    [SerializeField] protected IButton button;
    [SerializeField] protected GameObject titleObject;

    public void SetInfoPromptActive(bool active) {
        infoPrompt?.SetActive(active);
    }

    public void SetInfoPromptText(string text) {
        infoPrompt?.SetInfoPromptWithAutoSize(text, TextAnchor.MiddleCenter);
    }

    public void SetTitleActive(bool active) {
        titleObject?.SetActive(active);
    }

    public void GoToMap(int mapId) {
        TeleportHandler.Teleport(mapId);
    }

    public void OpenModMapPanel() {
        if (!SaveSystem.TryLoadPanelMod("WorldMap", out _)) {
            Hintbox.OpenHintboxWithContent("加载 Mod 地图失败", 16);
            return;
        }
            
        Panel.OpenPanel("[Mod]WorldMap");
    }
}
