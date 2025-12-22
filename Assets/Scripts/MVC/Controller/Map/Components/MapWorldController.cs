using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapWorldController : UIModule
{
    public void SetInfoPromptText(string content) {
        infoPrompt.SetInfoPromptWithAutoSize(content, TextAnchor.MiddleCenter);
        infoPrompt.SetPositionOffset(new Vector2(2, 2));
    }

    public void OpenWorldMapPanel() {
        if (Map.IsMod(Player.instance.currentMapId) && SaveSystem.TryLoadPanelMod("WorldMap", out _)) {
            Panel.OpenPanel("[Mod]WorldMap");
            return;
        }

        var worldId = (Player.instance.currentMap.categoryId - 1) / 100;
        var panel = Panel.OpenPanel<WorldMapPanel>();
        panel.SetPanelIdentifier("world", worldId.ToString());
    }

    public void OpenMiniMapPanel() {
        int categoryId = Player.instance.currentMap.categoryId;

        if (Map.IsMod(Player.instance.currentMapId) && SaveSystem.TryLoadPanelMod("MiniMap" + categoryId, out _)) {
            Panel.OpenPanel("[Mod]MiniMap" + categoryId);
            return;
        }

        Panel panel = Panel.OpenPanel("MiniMap" + categoryId);
        if (panel == null)
            Panel.OpenPanel("MiniMap1");
    }
}
