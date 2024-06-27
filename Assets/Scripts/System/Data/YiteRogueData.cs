using System;
using System.Xml.Serialization;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class YiTeRogueData
{
    public Pet[] petBag = new Pet[6];
    public List<int> buffIds = new List<int>();
    public List<Item> itemBag = new List<Item>();
    
    public int floor;
    public List<int> trace = new List<int>();
    public List<YiTeRogueEvent> eventMap = new List<YiTeRogueEvent>();
    
    
}

public class YiTeRogueEvent 
{
    public YiTeRogueEventType type;
    public int pos, step;
    public List<int> next;
    public List<IKeyValuePair<string, string>> data;

    public int eventIconBuffId => -13 - ((int)type);

    public static int GetEndStepByFloor(int floor) {
        return floor switch {
            _ => 8
        };
    }

    public static YiTeRogueEvent GetStartEvent(int floor) {
        var nextCount = Random.Range(0, 3);

        return new YiTeRogueEvent() {
            type = (floor == 0) ? YiTeRogueEventType.Start : YiTeRogueEventType.Heal,
            pos = 0,
            step = 0,
            next = nextCount switch {
                1 => new List<int>() { -1, 1 },
                2 => new List<int>() { -1, 0, 1 },
                _ => new List<int>() { 0 }, 
            },
            data = new List<IKeyValuePair<string, string>>(),
        };
    }

    public static YiTeRogueEvent GetEndEvent(int floor) {
        return new YiTeRogueEvent() {
            type = YiTeRogueEventType.End,
            pos = 0,
            step = GetEndStepByFloor(floor),
            next = null,
            data = new List<IKeyValuePair<string, string>>(),
        };
    }

    public static List<YiTeRogueEvent> GetNextEventList(int floor, YiTeRogueEvent previousEvent) {
        var nextEventList = new List<YiTeRogueEvent>();
        var endStep = GetEndStepByFloor(floor);

        if (previousEvent.step == endStep)
            return null;

        if (previousEvent.step == endStep - 1)
            return new List<YiTeRogueEvent>() { GetEndEvent(floor) };
        
        for (int i = 0; i < previousEvent.next.Count; i++) {
            var nextEvent = new YiTeRogueEvent();

            nextEvent.type = (YiTeRogueEventType)(Random.Range(2, 6));
            nextEvent.pos = previousEvent.pos + previousEvent.next[i];
            nextEvent.step = previousEvent.step + 1;
            nextEvent.data = new List<IKeyValuePair<string, string>>();

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

    public void Trigger() {

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
    Choose = 5,
}
