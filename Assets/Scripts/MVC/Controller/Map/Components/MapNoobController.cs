using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapNoobController : UIModule
{
    [SerializeField] private RectTransform rectTransform;
    [SerializeField] private IButton sevenDayLoginIcon;

    private bool isExtended = true;

    public override void Init()
    {
        base.Init();
        InitSevenDayLoginActivity();
    }

    private void InitSevenDayLoginActivity() {
        Activity activity = Activity.Find("noob_reward");
        int signedDays = int.Parse(activity.GetData("signedDays", "0"));
        sevenDayLoginIcon?.gameObject.SetActive(signedDays < 7);
    }

    public void SetInfoPromptText(string content) {
        infoPrompt.SetInfoPromptWithAutoSize(content, TextAnchor.MiddleCenter);
        infoPrompt.SetPosition(new Vector2(2, 2));
    }

    public void OpenPanel(string panelName) {
        Panel.OpenPanel(panelName);
    }

    public void GoToMap(int mapId) {
        TeleportHandler.Teleport(mapId);
    }

    public void Extend() {
        var x = isExtended ? -1000 : 0;
        rectTransform.anchoredPosition = new Vector2(x, rectTransform.anchoredPosition.y);
        isExtended = !isExtended;
    }
}
