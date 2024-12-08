using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapNoobController : UIModule
{
    [SerializeField] private RectTransform rectTransform;
    [SerializeField] private IButton sevenDayLoginIcon;
    [SerializeField] private List<IButton> activityButtons;
    [SerializeField] private List<string> activityIconPaths;
    [SerializeField] private NoobIntroController mapIntroController, trainIntroController;

    private bool isExtended = true;

    public override void Init()
    {
        base.Init();
        InitSevenDayLoginActivity();
        InitActivityButtons();
        InitNoobControllers();
    }

    private void InitSevenDayLoginActivity() {
        Activity activity = Activity.Find("noob_reward");
        var lastSignedDate = DateTime.Parse(activity.GetData("lastSignedDate", DateTime.MinValue.Date.ToString())).Date;
        var signedDays = int.Parse(activity.GetData("signedDays", "0"));
        if (Player.instance.gameData.IsNoob() || (signedDays > 7) || ((DateTime.Now.Date - lastSignedDate).Days < 1))
            return;

        Panel.OpenPanel<SignRewardPanel>();
        // int signedDays = int.Parse(activity.GetData("signedDays", "0"));
        // sevenDayLoginIcon?.gameObject.SetActive(signedDays < 7);
    }

    private void InitActivityButtons() {
        for (int i = 0; i < activityButtons.Count; i++) {
            activityButtons[i].gameObject.SetActive(i < activityIconPaths.Count);
            if (i >= activityIconPaths.Count)
                continue;
            
            var icon = NpcInfo.GetIcon(activityIconPaths[i]);
            activityButtons[i].transform.GetChild(0).GetComponent<Image>().SetSprite(icon);
        }
    }

    private void InitNoobControllers() {
        switch (Player.instance.gameData.noobCheckPoint) {
            default:
                return;
            case NoobCheckPoint.PetBag:
                Panel.OpenPanel<NoobPanel>();
                return;
            case NoobCheckPoint.Map:
                mapIntroController.gameObject.SetActive(true);
                return;
            case NoobCheckPoint.Train:
                trainIntroController.gameObject.SetActive(true);
                return;
            case NoobCheckPoint.Battle:
                Map.TestBattle(2, 201, "default");
                return;
        }
    }

    public void SetInfoPromptText(string content) {
        infoPrompt.SetInfoPromptWithAutoSize(content, TextAnchor.MiddleCenter);
        infoPrompt.SetPosition(new Vector2(2, 2));
    }

    public void Extend() {
        var x = isExtended ? -1000 : 0;
        rectTransform.anchoredPosition = new Vector2(x, rectTransform.anchoredPosition.y);
        isExtended = !isExtended;
    }
}
