using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapMenuController : UIModule
{
    [SerializeField] private Image mailNewIcon;

    public override void Init()
    {
        base.Init();
        mailNewIcon?.gameObject.SetActive(Player.instance.gameData.mailStorage.Any(x => !x.isRead));
    }

    public void SetInfoPromptText(string content) {
        infoPrompt.SetInfoPromptWithAutoSize(content, TextAnchor.MiddleCenter);
        infoPrompt.SetPosition(new Vector2(2, 2));
    }

    public void GoToStation() {
        GoToMap(Player.instance.currentMap.worldId * 10000 + 81);
    }

    public void OpenSPTPanel() {
        var panel = Panel.OpenPanel<SPTBossPanel>();
        panel?.SetPage(Player.instance.currentMap.worldId);
    }
}
