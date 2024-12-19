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

    public int mode;
    public bool isEnd = false;
    public Pet[] petBag = new Pet[6];
    public List<int> buffIds = new List<int>();
    public List<Item> itemBag = new List<Item>();
    
    public int floor;
    public int nextPos = int.MaxValue;
    public List<int> trace = new List<int>();
    public List<YiTeRogueEvent> eventMap = new List<YiTeRogueEvent>();

    [XmlIgnore] public YiTeRogueMode difficulty => (YiTeRogueMode)mode;
    [XmlIgnore] public BattlePet[] battlePetBag => petBag.Select(GetBattlePet).ToArray();
    [XmlIgnore] public bool isNextPosLocked => nextPos.IsWithin(-2, 2);
    [XmlIgnore] public Item prize => new Item(5, GetPrizeNum());

    public BattlePet GetBattlePet(Pet pet) {
        if (pet == null)
            return null;
    
        var copy = new Pet(pet);
        copy.feature.afterwardBuffIds.AddRange(buffIds);
        return BattlePet.GetBattlePet(copy);
    }

    public int GetPrizeNum() {
        var prizeFloor = floor;
        var prizeStep = trace.Count;

        if ((prizeFloor == 0) && (trace.Count <= 1))
            return 0;

        if (prizeFloor <= YiTeRogueEvent.GetEndFloorByDifficulty(difficulty)) {
            prizeFloor -= (trace.Count == 0) ? 1 : 0;
            prizeStep = (trace.Count == 0) ? YiTeRogueEvent.GetEndStepByFloor(prizeFloor) : (prizeStep - 1);
        }
        return prizeFloor * YiTeRogueEvent.GetEndStepByFloor(prizeFloor) * 2 + prizeStep + 2;
    }

    public YiTeRogueData(){}

    public YiTeRogueData(YiTeRogueMode difficulty) {
        mode = (int)difficulty;
        floor = 0;
        // 20到200HP各5瓶、350HP的2瓶、高級復活藥1瓶
        itemBag = Enumerable.Range(10011, 4).Select(x => new Item(x, 5))
            .Append(new Item(10016, 2)).Append(new Item(10018, 1)).ToList();
            
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
}

public enum YiTeRogueMode {
    Test = -1,
    None = 0,
    Easy = 1,
    Hard = 2,
}
