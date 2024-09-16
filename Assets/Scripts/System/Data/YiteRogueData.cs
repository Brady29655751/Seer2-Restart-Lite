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

    public Pet[] petBag = new Pet[6];
    public List<int> buffIds = new List<int>();
    public List<Item> itemBag = new List<Item>();
    
    public int floor;
    public List<int> trace = new List<int>();
    public List<YiTeRogueEvent> eventMap = new List<YiTeRogueEvent>();

    public void CreateRogue() {
        var startEvent = YiTeRogueEvent.GetStartEvent(floor);
        var eventQueue = new Queue<YiTeRogueEvent>();
        var eventMap = new List<YiTeRogueEvent>(){ startEvent };

        eventQueue.Enqueue(startEvent);

        while (eventQueue.Count > 0) {
            var peekEvent = eventQueue.Dequeue();
            var nextEventList = YiTeRogueEvent.GetNextEventList(floor, peekEvent)
                ?.Where(x => !eventMap.Exists(y => (x.pos == y.pos) && (x.step == y.step))).ToList();

            if (nextEventList == null)
                break;

            nextEventList.ForEach(eventQueue.Enqueue);
            eventMap.AddRange(nextEventList);
        }

        this.eventMap = eventMap;
        SaveSystem.SaveData();
    }
    
    // Return this floor is finished or not.
    public bool Step(int stepPos) {
        trace.Add(stepPos);

        // If this floor is not finished, continue.
        if (trace.Count <= YiTeRogueEvent.GetEndStepByFloor(floor)) {
            SaveSystem.SaveData();
            return false;
        }
        // Else, end this floor.
        floor += 1;
        CreateRogue();
        return true;
    }
}
