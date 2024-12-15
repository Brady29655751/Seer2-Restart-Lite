using System;
using System.Xml.Serialization;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class YiTeRogueEvent 
{
    public YiTeRogueEventType type;
    public int pos, step;
    public List<int> next;
    public List<IKeyValuePair<string, string>> data;

    [XmlIgnore] public int eventIconBuffId => GetEventIconBuffId(type);
    [XmlIgnore] public int npcId => GetEventNpcId(type);
    [XmlIgnore] public string title => GetData("title");
    [XmlIgnore] public string content => GetData("content");
    [XmlIgnore] public List<YiTeRogueChoice> choiceList => YiTeRogueChoice.GetChoiceList(this);

    public string GetData(string key, string defaultReturn = null) {
        return data.Find(x => x.key == key)?.value ?? defaultReturn;
    }

    public void SetData(string key, string value) {
        var dataToSet = data.Find(x => x.key == key);
        if (dataToSet == null)
            return;

        dataToSet.value = value;
    }

    public static int GetEventIconBuffId(YiTeRogueEventType type) => -13 - ((int)type);
    public static int GetEventNpcId(YiTeRogueEventType type) => YiTeRoguePanel.ROGUE_NPC_ID - GetEventIconBuffId(type) - 1;

    public static int GetEndFloorByDifficulty(YiTeRogueMode difficulty) {
        return difficulty switch {
            YiTeRogueMode.Test => 2,
            YiTeRogueMode.Easy => 2,
            YiTeRogueMode.Hard => 4,
            _ => 0,
        };
    }

    public static int GetEndStepByFloor(int floor) {
        return floor switch {
            _ => 8
        };
    }

    public static YiTeRogueEvent GetStartEvent() {
        var floor = YiTeRogueData.instance.floor;
        var nextCount = Random.Range(0, 3);
        var type = (floor == 0) ? YiTeRogueEventType.Start : YiTeRogueEventType.Heal;

        return new YiTeRogueEvent() {
            type = type,
            pos = 0,
            step = 0,
            next = nextCount switch {
                1 => new List<int>() { -1, 1 },
                2 => new List<int>() { -1, 0, 1 },
                _ => new List<int>() { 0 }, 
            },
            data = GetYiTeRogueEventData(type),
        };
    }

    public static YiTeRogueEvent GetEndEvent() {
        var floor = YiTeRogueData.instance.floor;
        return new YiTeRogueEvent() {
            type = YiTeRogueEventType.End,
            pos = 0,
            step = GetEndStepByFloor(floor),
            next = null,
            data = GetYiTeRogueEventData(YiTeRogueEventType.End),
        };
    }

    public static List<YiTeRogueEvent> GetNextEventList(YiTeRogueEvent previousEvent) {
        var nextEventList = new List<YiTeRogueEvent>();
        var floor = YiTeRogueData.instance.floor;
        var endStep = GetEndStepByFloor(floor);

        if (previousEvent.step == endStep)
            return null;

        if (previousEvent.step == endStep - 1)
            return new List<YiTeRogueEvent>() { GetEndEvent() };
        
        for (int i = 0; i < previousEvent.next.Count; i++) {
            var nextEvent = new YiTeRogueEvent();

            nextEvent.type = (YiTeRogueEventType)Random.Range(2, 6);
            nextEvent.pos = previousEvent.pos + previousEvent.next[i];
            nextEvent.step = previousEvent.step + 1;
            nextEvent.data = GetYiTeRogueEventData(nextEvent.type);

            if (nextEvent.step == endStep - 1)
                nextEvent.next = new List<int>() { 0 - nextEvent.pos };
            else {
                nextEvent.next = new List<int>() { -1, 0, 1 };
                if (nextEvent.step == endStep - 2)
                    nextEvent.next.RemoveAll(x => !((nextEvent.pos + x).IsWithin(-1, 1)));
                else {
                    var up = (nextEvent.pos > -2) && (Random.Range(0, 2) == 0);
                    var down = (nextEvent.pos < 2) && (Random.Range(0, 2) == 0);
                    var zero = ((!up) && (!down)) || (Random.Range(0, 2) == 0);
                    if (!up)
                        nextEvent.next.Remove(-1);
                    if (!down)
                        nextEvent.next.Remove(1);
                    if (!zero)
                        nextEvent.next.Remove(0);
                }
            }
            nextEventList.Add(nextEvent);
        }
        return nextEventList;
    }

    private static List<IKeyValuePair<string, string>> GetYiTeRogueEventData(YiTeRogueEventType type) {
        var data = new List<IKeyValuePair<string, string>>();
        var difficulty = YiTeRogueData.instance.difficulty;
        var floor = YiTeRogueData.instance.floor;
        var isEndRogue = floor == YiTeRogueEvent.GetEndFloorByDifficulty(difficulty);
        var battleMapId = (floor < 2) ? "83" : "84";
        var randomFloor = Identifier.GetNumIdentifier("random[1~6|8~13|15~20]").ToString();
        var randomBoss = (Random.Range(1, 4) * 7).ToString();
        var randomDialog = type == YiTeRogueEventType.Dialog ? 1.ToString() : 1.ToString();

        switch (type) {
            default:
                break;
            case YiTeRogueEventType.Start:
                var petId = GameManager.versionData.petData.petDictionary.GroupBy(x => x.basic.baseId).First()
                    .Select(x => x.id.ToString()).ToList().Random(3, false).ConcatToString("/");
                data.Add(new IKeyValuePair<string, string>("title", (floor == 0) ? "起始点" : "休息站"));
                data.Add(new IKeyValuePair<string, string>("content", (floor == 0) ? "选择你的起始伊特吧！" : "回复体力并选择你的新伙伴吧！"));    
                data.Add(new IKeyValuePair<string, string>("pet", (floor == 0) ? "100/810" : petId));
                break;
            case YiTeRogueEventType.BattleEasy:
                data.Add(new IKeyValuePair<string, string>("title", "战斗（简单）"));
                data.Add(new IKeyValuePair<string, string>("content", "点击下方选项进入战斗"));
                data.Add(new IKeyValuePair<string, string>("map_id", battleMapId));
                data.Add(new IKeyValuePair<string, string>("npc_id", battleMapId + "01"));
                data.Add(new IKeyValuePair<string, string>("battle_id", randomFloor));
                data.Add(new IKeyValuePair<string, string>("result", "none"));
                data.Add(new IKeyValuePair<string, string>("reward", "10201/10221/10231"));
                break;
            case YiTeRogueEventType.BattleHard:
                data.Add(new IKeyValuePair<string, string>("title", "战斗（困难）"));
                data.Add(new IKeyValuePair<string, string>("content", "点击下方选项进入战斗"));
                data.Add(new IKeyValuePair<string, string>("map_id", battleMapId));
                data.Add(new IKeyValuePair<string, string>("npc_id", battleMapId + "02"));
                data.Add(new IKeyValuePair<string, string>("battle_id", randomFloor));
                data.Add(new IKeyValuePair<string, string>("result", "none"));
                data.Add(new IKeyValuePair<string, string>("reward", "10205/10225/10235"));
                break;
            case YiTeRogueEventType.Reward:
            case YiTeRogueEventType.Dialog:
                var dialogNpc = Map.GetNpcInfo(Player.instance.currentMap, YiTeRogueEvent.GetEventNpcId(type));
                var dialog = dialogNpc.dialogHandler.Find(x => x.id == randomDialog);
                data.Add(new IKeyValuePair<string, string>("title", dialog.name));
                data.Add(new IKeyValuePair<string, string>("content", dialog.content));
                data.Add(new IKeyValuePair<string, string>("dialog_id", dialog.id));
                break;
            case YiTeRogueEventType.End:
                data.Add(new IKeyValuePair<string, string>("title", "战斗（终极）"));
                data.Add(new IKeyValuePair<string, string>("content", "点击下方选项进入战斗"));
                data.Add(new IKeyValuePair<string, string>("map_id", "84"));
                data.Add(new IKeyValuePair<string, string>("npc_id", "8402"));
                data.Add(new IKeyValuePair<string, string>("battle_id", randomBoss));
                data.Add(new IKeyValuePair<string, string>("result", "none"));
                data.Add(new IKeyValuePair<string, string>("reward", isEndRogue ? "5" : "2/3/4"));
                break;
        };
        return data;
    }

}

public enum YiTeRogueEventType {
    Store = -2,
    End = -1,
    Start = 0,
    Heal = 1,
    BattleEasy = 2,
    BattleHard = 3,
    Reward = 4,
    Dialog = 5,
}