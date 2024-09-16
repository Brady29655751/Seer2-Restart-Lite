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

    [XmlIgnore] public int eventIconBuffId => -13 - ((int)type);
    [XmlIgnore] public string title => data.Find(x => x.key == "title")?.value;
    [XmlIgnore] public string content => data.Find(x => x.key == "content")?.value;
    [XmlIgnore] public List<YiTeRogueChoice> choiceList => YiTeRogueChoice.GetChoiceList(this);

    public string GetData(string key, string defaultReturn = null) {
        return data.Find(x => x.key == key)?.value ?? defaultReturn;
    }

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
            data = new List<IKeyValuePair<string, string>>() {
                new IKeyValuePair<string, string>("title", (floor == 0) ? "起始点" : "休息站"),
                new IKeyValuePair<string, string>("content", (floor == 0) ? "选择你的起始伊特吧！" : "回复体力并选择你的新伙伴吧！"),
                new IKeyValuePair<string, string>("pet", (floor == 0) ? "3/6/9" : "-3/-6/-9"),
            },
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