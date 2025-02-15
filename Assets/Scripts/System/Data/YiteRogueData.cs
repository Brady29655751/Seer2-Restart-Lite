using System;
using System.Xml.Serialization;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class YiTeRogueData
{
    [XmlIgnore] public static YiTeRogueData instance => Player.instance.gameData.yiteRogueData;
    [XmlIgnore] public static Dictionary<YiTeRogueMode, string> modeNameDict = new Dictionary<YiTeRogueMode, string>() {
        { YiTeRogueMode.Normal, "普通" },
        { YiTeRogueMode.Endless,"无尽" },
        { YiTeRogueMode.Mod,    "Mod" },
    };

    public int mode;
    public bool isEnd = false;
    public Pet[] petBag = new Pet[6];
    public Status buffStatus = Status.zero;
    public List<int> buffIds = new List<int>();
    public List<Item> itemBag = new List<Item>();
    
    public int floor;
    public int nextPos = int.MaxValue;
    public List<int> trace = new List<int>();
    public List<YiTeRogueEvent> eventMap = new List<YiTeRogueEvent>();

    [XmlIgnore] public YiTeRogueMode difficulty => (YiTeRogueMode)mode;
    [XmlIgnore] public BattlePet[] battlePetBag => petBag.Select(GetBattlePet).ToArray();
    [XmlIgnore] public List<Buff> initBuffs => GetStatusBuffs().Concat(buffIds.Select(x => new Buff(x))).ToList();
    [XmlIgnore] public bool isNextPosLocked => nextPos.IsWithin(-2, 2);
    [XmlIgnore] public Item prize => new Item(5, GetPrizeNum());

    public static string GetModeName(YiTeRogueMode mode) => modeNameDict.Get(mode, "未知");

    public List<Buff> GetStatusBuffs() {
        var statusBuffs = new List<Buff>();
        for (int i = 0; i < 5; i++) {
            var status = (int)buffStatus[i];
            if (status != 0)
                statusBuffs.Add(new Buff(400015 + i, -1, status));
        }
        return statusBuffs;
    }

    public BattlePet GetBattlePet(Pet pet) {
        if (pet == null)
            return null;
    
        var copy = new Pet(pet);
        copy.feature.afterwardBuffIds.AddRange(buffIds);
        var battlePet = BattlePet.GetBattlePet(copy);
        for (int i = 0; i < 5; i++) {
            var status = (int)buffStatus[i];
            if (status != 0)
                battlePet.buffs.Add(new Buff(400015 + i, -1, status));
        }
        return battlePet;
    }

    public int GetPrizeNum() {
        var prizeFloor = floor;
        var prizeStep = trace.Count;

        if (((prizeFloor == 0) && (trace.Count <= 2)) || (difficulty != YiTeRogueMode.Normal))
            return 0;

        if (prizeFloor <= YiTeRogueEvent.GetEndFloorByDifficulty(difficulty)) {
            prizeFloor -= (trace.Count == 0) ? 1 : 0;
            prizeStep = (trace.Count == 0) ? YiTeRogueEvent.GetEndStepByFloor(prizeFloor) : (prizeStep - 1);
        }
        // Floor 0: 20 + step
        // Floor 1: 44 + step
        // Floor 2: 81 + step
        // Floor 3: 125 + step
        return (prizeFloor + 1) * YiTeRogueEvent.GetEndStepByFloor(prizeFloor) * 3 / 2 + prizeStep + 5;
    }

    public YiTeRogueData(){}

    public YiTeRogueData(YiTeRogueMode difficulty) {
        mode = (int)difficulty;
        floor = 0;
        // 20到200HP各5瓶、350HP的2瓶、高級復活藥1瓶
        itemBag = Enumerable.Range(10011, 4).Select(x => new Item(x, 5))
            .Append(new Item(10016, 2)).Append(new Item(10018, 1)).Append(new Item(6, 50)).ToList();
            
        // 随机1项普通初始Buff
        buffIds = 440000.SingleToList();
    }

    public static YiTeRogueData CreateRogue(YiTeRogueMode difficulty) {
        Player.instance.gameData.yiteRogueData = new YiTeRogueData(difficulty);
        YiTeRogueData.instance.CreateMap();
        return YiTeRogueData.instance;
    }

    public void CreateMap() {
        trace.Clear();
        if (floor > YiTeRogueEvent.GetEndFloorByDifficulty(difficulty)) {
            isEnd = true;
            SaveSystem.SaveData();
            return;
        }

        var startEvent = YiTeRogueEvent.GetStartEvent();
        var eventQueue = new Queue<YiTeRogueEvent>();
        var eventMap = new List<YiTeRogueEvent>(){ startEvent };

        eventQueue.Enqueue(startEvent);

        while (eventQueue.Count > 0) {
            var peekEvent = eventQueue.Dequeue();
            var nextEventList = YiTeRogueEvent.GetNextEventList(peekEvent)
                ?.Where(x => !eventMap.Exists(y => (x.pos == y.pos) && (x.step == y.step))).ToList();

            if (nextEventList == null)
                break;

            nextEventList.ForEach(eventQueue.Enqueue);
            eventMap.AddRange(nextEventList);
        }

        this.eventMap = eventMap;
        SaveSystem.SaveData();
    }
    
    /// <summary>
    /// Click a rogue event and lock. Player can only choose this event even if close the panel.
    /// </summary>
    /// <param name="stepPos">The clicked event pos</param>
    /// <returns>this event is clicked already or not</returns>
    public bool Click(int stepPos) {
        var isClicked = nextPos == stepPos;
        nextPos = stepPos;
        return isClicked;
    }

    /// <summary>
    /// Finish a rogue event and step on it.
    /// </summary>
    /// <param name="stepPos">The finished event pos</param>
    /// <returns>this floor is finished or not</returns>
    public bool Step(int stepPos) {
        trace.Add(stepPos);
        nextPos = int.MaxValue;

        // If this floor is not finished, continue.
        if (trace.Count <= YiTeRogueEvent.GetEndStepByFloor(floor)) {
            SaveSystem.SaveData();
            return false;
        }
        // Else, end this floor.
        floor += 1;
        CreateMap();
        return true;
    }

    public float GetYiTeRogueIdentifier(string id) {
        return id switch {
            "mode"          => mode,
            "difficulty"    => mode,
            _               => 0,
        };
    }
}

public enum YiTeRogueMode {
    Test = -1,
    None = 0,
    Normal = 1,
    Endless = 2,
    Mod = 3,
}
