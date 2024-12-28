using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class YiTeRoguePanel : Panel
{
    [SerializeField] private YiTeRogueView rogueView;
    [SerializeField] private PetBagPanel petBagPanel;

    public YiTeRogueData rogueData => YiTeRogueData.instance;
    public const int ROGUE_NPC_ID = 50001;

    public override void Init() {
        Player.instance.currentNpcId = ROGUE_NPC_ID;
        SetMode(YiTeRogueData.instance?.difficulty ?? YiTeRogueMode.None);
    }

    public override void SetPanelIdentifier(string id, string param) {
        switch (id) {
            default:
                base.SetPanelIdentifier(id, param);
                return;
            case "mode":
                SetMode((YiTeRogueMode)int.Parse(param));
                return;
        }
    }

    public void SetMode(YiTeRogueMode mode) {
        if ((rogueData == null) || rogueData.isEnd || (rogueData.difficulty <= YiTeRogueMode.None))
            YiTeRogueData.CreateRogue(mode);
        else if (rogueData.difficulty != mode) {
            var currentModeName = YiTeRogueData.GetModeName(rogueData.difficulty);
            var newModeName = YiTeRogueData.GetModeName(mode);
            var hintbox = Hintbox.OpenHintboxWithContent("当前为<color=#ffbb33>【" + currentModeName + "】</color>模式\n" +
                "想游玩<color=#ffbb33>【" + newModeName + "】</color>模式请先点击<color=red>【重新开始】</color>", 16);
            hintbox.SetSize(420, 240);
        }
        rogueView.SetMap();
    }

    public void Terminate() {
        var hintbox = Hintbox.OpenHintbox();
        hintbox.SetTitle("提示");
        hintbox.SetContent("确定要放弃当前进度并结算吗？\n下次游玩将从零开始哦！", 16, FontOption.Arial);
        hintbox.SetOptionNum(2);
        hintbox.SetOptionCallback(() => {
            var prize = YiTeRogueData.instance.prize;
            Item.Add(prize);
            Item.OpenHintbox(prize);
            Hintbox.OpenHintboxWithContent("你止步于第 " + (YiTeRogueData.instance.floor + 1) + " 层，下次继续努力吧！", 16);
            Player.instance.gameData.yiteRogueData = null;
            SaveSystem.SaveData();
            ClosePanel();
        });
    }

    public void OpenChoicePanel(YiTeRogueEvent rogueEvent, bool withStep = true) {
        rogueView.OpenChoicePanel(rogueEvent, withStep);
    }

    public void CloseChoicePanel() {
        rogueView.CloseChoicePanel();
        if (!YiTeRogueData.instance.isEnd)
            return;

        ClosePanel();
    }

    public void OpenPetBag() {
        if (rogueData.petBag.All(x => x == null)) {
            Hintbox.OpenHintboxWithContent("背包中还没有任何精灵", 16);
            return;
        }
        petBagPanel?.SetActive(true);
        petBagPanel?.SetPetBag(rogueData.petBag);
        petBagPanel?.SetItemBag(rogueData.itemBag);
    }

    public void ClosePetBag() {
        petBagPanel?.SetActive(false);
        Player.instance.gameData.yiteRogueData.petBag = petBagPanel.petBag;
        SaveSystem.SaveData();
    }

    public void CheckCurrentBuffs() {
        rogueView.ToggleBuffPanel();
    }
    
}
