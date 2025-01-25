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

    [XmlIgnore] public Sprite eventIcon => GetEventIcon();
    [XmlIgnore] public int eventIconBuffId => GetEventTypeIconId(type);
    [XmlIgnore] public int npcId => GetEventNpcId(type);
    [XmlIgnore] public int battleDifficulty => GetBattleDifficulty(type);
    [XmlIgnore] public string title => GetData("title");
    [XmlIgnore] public string content => GetData("content");
    [XmlIgnore] public List<YiTeRogueChoice> choiceList => YiTeRogueChoice.GetChoiceList(this);
    [XmlIgnore] public List<YiTeRogueEvent> nextEventList => YiTeRogueData.instance?.eventMap
        .FindAll(x => (x.step == step + 1) && (next?.Exists(y => x.pos == pos + y) ?? false));

    public string GetData(string key, string defaultReturn = null) {
        return data.Find(x => x.key == key)?.value ?? defaultReturn;
    }

    public void SetData(string key, string value) {
        var dataToSet = data.Find(x => x.key == key);
        if (dataToSet == null)
            return;

        dataToSet.value = value;
    }

    public Sprite GetEventIcon() {
        return (type == YiTeRogueEventType.End)
        ? choiceList.FirstOrDefault()?.icon 
        : SpriteSet.GetIconSprite("buff[" + eventIconBuffId + "]");
    }

    public static int GetEventTypeIconId(YiTeRogueEventType type) => -13 - ((int)type);
    public static int GetEventNpcId(YiTeRogueEventType type) => YiTeRoguePanel.ROGUE_NPC_ID - GetEventTypeIconId(type) - 1;
    public static int GetBattleDifficulty(YiTeRogueEventType type) => type switch {
        YiTeRogueEventType.BattleEasy => 0,
        YiTeRogueEventType.BattleHard => 1,
        YiTeRogueEventType.End => 2,
        _ => -1,
    };

    public static int GetEndFloorByDifficulty(YiTeRogueMode difficulty) {
        return difficulty switch {
            YiTeRogueMode.Test => 2,
            YiTeRogueMode.Normal => 2,
            YiTeRogueMode.Endless => int.MaxValue,
            YiTeRogueMode.Mod => int.MaxValue,
            _ => 0,
        };
    }

    public static int GetEndStepByFloor(int floor) {
        if (floor >= 3)
            return 20;

        if (floor == 2)
            return 17;

        return (floor == 0) ? 10 : 13;
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
                // 1 => new List<int>() { -1, 1 },
                // 2 => new List<int>() { -1, 0, 1 },
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

            nextEvent.type = GetNextEventType(previousEvent, endStep);
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

    public static YiTeRogueEventType GetNextEventType(YiTeRogueEvent previousEvent, int endStep) {
        var previousStep = previousEvent.step;
        if (previousStep % 5 == 0)
            return YiTeRogueEventType.Store;

        var hardRatio = Mathf.Min(previousStep / 5, 3);
        return (Random.Range(0, hardRatio + 2) == 0) ? YiTeRogueEventType.BattleEasy : YiTeRogueEventType.BattleHard;
    }   

    public static List<IKeyValuePair<string, string>> GetYiTeRogueEventData(YiTeRogueEventType type) {
        var data = new List<IKeyValuePair<string, string>>();
        var petBag = YiTeRogueData.instance.petBag;
        var difficulty = YiTeRogueData.instance.difficulty;
        var floor = YiTeRogueData.instance.floor;
        var isEndRogue = floor == YiTeRogueEvent.GetEndFloorByDifficulty(difficulty);

        var extraFloor = (difficulty == YiTeRogueMode.Mod) ? 0 : Mathf.Min(floor, YiTeRogueEvent.GetEndFloorByDifficulty(YiTeRogueMode.Normal) + 1);
        var battleMapId = (82 + extraFloor).ToString();
        var randomFloor = Identifier.GetNumIdentifier("random[1~6|8~13|15~20]").ToString();
        var randomBoss = (Random.Range(1, 4) * 7).ToString();
        var randomDialog = type == YiTeRogueEventType.Dialog ? 1.ToString() : 1.ToString();

        var petData = GameManager.versionData.petData;

        switch (type) {
            default:
                break;
            case YiTeRogueEventType.Start:
            case YiTeRogueEventType.Heal:
                var petDict = (difficulty == YiTeRogueMode.Mod) ? petData.petModLastEvolveDictionary : petData.petLastEvolveDictionary;
                var petId = petDict.Where(x => x.basic.baseId != 91).Select(x => x.id.ToString()).ToList();
                var yiteId = (difficulty == YiTeRogueMode.Mod) ? petId : Pet.GetPetInfo(91).exp.evolvePetIds.Select(x => x.ToString()).ToList();
                var title = (floor == 0) ? "起始点" : "休息站";
                var content = petBag.All(x => x != null) ? "回复体力吧！" : ((floor == 0) ? "选择你的起始精灵吧！" : "回复体力并选择你的新伙伴吧！");
                var pet = petBag.All(x => x != null) ? "none" : ((floor == 0) ? yiteId : petId).Random(3, false).ConcatToString("/");
                data.Add(new IKeyValuePair<string, string>("title", title));
                data.Add(new IKeyValuePair<string, string>("content", content));    
                data.Add(new IKeyValuePair<string, string>("pet", pet));
                break;
            case YiTeRogueEventType.Store:
                var randomGrow = Item.yiteGrowItemDatabse.Where(x => x.id % 5 < 4).ToList().Random(3, false).Select(x => x.id.ToString());
                var randomSkill = Item.skillBookItemDatabase.Random(6, false).Select(x => x.id.ToString());
                var storeItems = randomGrow.Concat(randomSkill).ConcatToString("/");
                data.Add(new IKeyValuePair<string, string>("title", "神秘商店"));
                data.Add(new IKeyValuePair<string, string>("content", "突然出现在你眼前的小摊位，看看有什么好东西吧！"));    
                data.Add(new IKeyValuePair<string, string>("item", storeItems));
                break;
            case YiTeRogueEventType.BattleEasy:
                var easyGrowReward = Item.yiteGrowItemDatabse.Where(x => x.id % 5 < 2).ToList()
                    .Random(1, false).Select(x => "item[" + x.id + "]");
                var easyBuffReward = Buff.yiteEasyBuffDatabse.Where(x => (x.info.copyHandleType == CopyHandleType.New) || (!YiTeRogueData.instance.buffIds.Contains(x.id))).ToList()
                    .Random(2, false).Select(x => "buff[" + x.id + "]");
                var easyReward = easyGrowReward.Concat(easyBuffReward).ConcatToString("/");
                data.Add(new IKeyValuePair<string, string>("title", "战斗（简单）"));
                data.Add(new IKeyValuePair<string, string>("content", "点击下方选项进入战斗"));
                data.Add(new IKeyValuePair<string, string>("map_id", battleMapId));
                data.Add(new IKeyValuePair<string, string>("npc_id", battleMapId + "01"));
                data.Add(new IKeyValuePair<string, string>("battle_id", randomFloor));
                data.Add(new IKeyValuePair<string, string>("result", "none"));
                data.Add(new IKeyValuePair<string, string>("reward", easyReward));
                break;
            case YiTeRogueEventType.BattleHard:
                var hardGrowReward = Item.yiteGrowItemDatabse.Where(x => (x.id % 5).IsWithin(1, 3)).ToList()
                    .Random(1, false).Select(x => "item[" + x.id + "]");
                var hardBuffReward = Buff.yiteHardBuffDatabse.Where(x => (x.info.copyHandleType == CopyHandleType.New) || (!YiTeRogueData.instance.buffIds.Contains(x.id))).ToList()
                    .Random(2, false).Select(x => "buff[" + x.id + "]");
                var hardReward = hardGrowReward.Concat(hardBuffReward).ConcatToString("/");
                data.Add(new IKeyValuePair<string, string>("title", "战斗（困难）"));
                data.Add(new IKeyValuePair<string, string>("content", "点击下方选项进入战斗"));
                data.Add(new IKeyValuePair<string, string>("map_id", battleMapId));
                data.Add(new IKeyValuePair<string, string>("npc_id", battleMapId + "02"));
                data.Add(new IKeyValuePair<string, string>("battle_id", randomFloor));
                data.Add(new IKeyValuePair<string, string>("result", "none"));
                data.Add(new IKeyValuePair<string, string>("reward", hardReward));
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
                var endGrowReward = Item.yiteGrowItemDatabse.Where(x => x.id % 5 == 4).ToList()
                    .Random(1, false).Select(x => "item[" + x.id + "]");
                var endBuffReward = Buff.yiteEndBuffDatabse.Where(x => (x.info.copyHandleType == CopyHandleType.New) || (!YiTeRogueData.instance.buffIds.Contains(x.id))).ToList()
                    .Random(2, false).Select(x => "buff[" + x.id + "]");
                var endReward = endGrowReward.Concat(endBuffReward).ConcatToString("/");
                data.Add(new IKeyValuePair<string, string>("title", "战斗（终极）"));
                data.Add(new IKeyValuePair<string, string>("content", "点击下方选项进入战斗"));
                data.Add(new IKeyValuePair<string, string>("map_id", battleMapId));
                data.Add(new IKeyValuePair<string, string>("npc_id", battleMapId + "02"));
                data.Add(new IKeyValuePair<string, string>("battle_id", randomBoss));
                data.Add(new IKeyValuePair<string, string>("result", "none"));
                data.Add(new IKeyValuePair<string, string>("reward", isEndRogue ? "item[5]" : endReward));
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