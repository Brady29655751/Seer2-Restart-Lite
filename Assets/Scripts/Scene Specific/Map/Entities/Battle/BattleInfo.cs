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
        settings.FixToYiTeRogue();
        playerInfo = null;
        enemyInfo.ForEach(x => {
            x.level = 5;//YiTeRogueData.instance.petBag.Max(x => x?.level ?? int.MinValue) + 5;
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
        SaveSystem.SaveData();
        Panel.OpenPanel<YiTeRoguePanel>().OpenChoicePanel(rogueEvent, false);
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
