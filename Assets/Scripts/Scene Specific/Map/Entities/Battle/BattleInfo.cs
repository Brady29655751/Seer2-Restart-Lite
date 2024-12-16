using System.Linq;
using System.Xml.Serialization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class BattleInfo 
{
    [XmlAttribute("id")] public string id;
    public string content = string.Empty;
    public BattleSettings settings = new BattleSettings();

    [XmlArray("player"), XmlArrayItem(typeof(BossInfo), ElementName = "pet")] 
    public List<BossInfo> playerInfo; 

    [XmlArray("enemy"), XmlArrayItem(typeof(BossInfo), ElementName = "pet")] 
    public List<BossInfo> enemyInfo;
    
    [XmlArray("winHandler"), XmlArrayItem(typeof(NpcButtonHandler), ElementName = "button")] 
    public List<NpcButtonHandler> winHandler;

    [XmlArray("loseHandler"), XmlArrayItem(typeof(NpcButtonHandler), ElementName = "button")] 
    public List<NpcButtonHandler> loseHandler;

    public BattleInfo FixToYiTeRogue(YiTeRogueEvent rogueEvent) {
        var petBag = YiTeRogueData.instance.petBag;
        settings.FixToYiTeRogue();
        playerInfo = null;
        enemyInfo.ForEach(x => {
            x.level = petBag.Max(x => x?.level ?? 100);
            x.status = null;
            x.hasEmblem = true;
        });
        winHandler = NpcButtonHandler.Callback(() => OnYiTeRogueBattleWin(rogueEvent)).SingleToList();
        loseHandler = NpcButtonHandler.Callback(() => OnYiTeRogueBattleLose(rogueEvent)).SingleToList();
        return this;
    }

    private void OnYiTeRogueBattleWin(YiTeRogueEvent rogueEvent) {
        rogueEvent.SetData("title", "战斗胜利");
        rogueEvent.SetData("content", "选择下列选项之一作为你的战利品！");
        rogueEvent.SetData("result", "win");

        foreach (var pet in YiTeRogueData.instance.petBag) {
            if (pet == null)
                continue;

            // 升1級，恢復1/8最大體力
            pet.GainExp(pet.levelUpExp, false);
            pet.currentStatus.hp += (int)(pet.normalStatus.hp / 8);
        }
        // 獲得3個冰糖，每個冰糖能+10學習力
        // 根據難度獲得餅乾，簡單+10、困難+20、Boss +40
        var currencyNum = rogueEvent.type switch {
            YiTeRogueEventType.BattleEasy   => 10,
            YiTeRogueEventType.BattleHard   => 20,
            YiTeRogueEventType.End          => 40,
            _ => 0,
        };
        var currency = new Item(6, currencyNum);
        var item = new Item(10232, 3);
        Item.AddTo(currency, YiTeRogueData.instance.itemBag);
        Item.AddTo(item, YiTeRogueData.instance.itemBag);
        Panel.OpenPanel<YiTeRoguePanel>().OpenChoicePanel(rogueEvent, false);
        Item.OpenHintbox(item);
        Item.OpenHintbox(currency);
    }

    private void OnYiTeRogueBattleLose(YiTeRogueEvent rogueEvent) {
        YiTeRogueData.instance.isEnd = true;
        rogueEvent.SetData("title", "战斗失败");
        rogueEvent.SetData("content", "路途要在这里结束了，下次再来挑战！\n点击下方选项领取奖励吧！");
        rogueEvent.SetData("result", "lose");
        SaveSystem.SaveData();
        Panel.OpenPanel<YiTeRoguePanel>().OpenChoicePanel(rogueEvent, false);
    }
}
