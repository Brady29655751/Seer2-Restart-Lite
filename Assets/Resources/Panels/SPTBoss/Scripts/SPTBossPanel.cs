using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SPTBossPanel : Panel
{
    [SerializeField] private List<GameObject> bossPages = new List<GameObject>();

    [SerializeField] private List<int> bossId;
    [SerializeField] private List<int> mapId;
    [SerializeField] private List<IButton> bossButtonList;

    public override void Init() {
        for (int i = 0; i < bossButtonList.Count; i++) {
            int copy = i;
            var info = Pet.GetPetInfo(bossId[copy]);
            if (info == null) {
                bossButtonList[i]?.gameObject.SetActive(false);
                continue;
            }
            bossButtonList[i]?.gameObject.SetActive(true);
            bossButtonList[i]?.SetSprite(info.ui.idleImage);
            bossButtonList[i]?.onPointerEnterEvent.SetListener(() => SetInfoPromptActive(true));
            bossButtonList[i]?.onPointerExitEvent.SetListener(() => SetInfoPromptActive(false));
            bossButtonList[i]?.onPointerOverEvent.SetListener(() => ShowBossInfo(info.name));
            bossButtonList[i]?.onPointerClickEvent.SetListener(() => GoToMap(mapId[copy]));
        }
    }

    public override void SetPanelIdentifier(string id, string param)
    {
        switch (id) 
        {
            default:
                base.SetPanelIdentifier(id, param);
                return;
            case "page":
                SetPage(int.Parse(param));
                return;
        }
        
    }

    public void SetPage(int page) {
        for (int i = 0; i < bossPages.Count; i++)
            bossPages[i].SetActive(i == page);
    }

    public void ShowBossInfo(string bossName) {
        infoPrompt.SetInfoPromptWithAutoSize(bossName, TextAnchor.MiddleLeft);
    }
}
