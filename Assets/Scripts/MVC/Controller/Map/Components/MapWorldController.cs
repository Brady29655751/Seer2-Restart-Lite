using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapWorldController : UIModule
{
    public void SetInfoPromptText(string content) {
        infoPrompt.SetInfoPromptWithAutoSize(content, TextAnchor.MiddleCenter);
        infoPrompt.SetPosition(new Vector2(2, 2));
    }

    public void OpenWorldMapPanel() {
        Panel.OpenPanel<WorldMapPanel>();
    }

    public void OpenMiniMapPanel() {
        int categoryId = Player.instance.currentMap.categoryId;
        Panel panel = Panel.OpenPanel("MiniMap" + categoryId);
        if (panel == null)
            Panel.OpenPanel("MiniMap1");
    }
}
