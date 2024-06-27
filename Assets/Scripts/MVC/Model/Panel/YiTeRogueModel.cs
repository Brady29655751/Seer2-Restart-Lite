using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class YiTeRogueModel : Module
{
    public YiTeRogueData rogueData => Player.instance.gameData.yiteRogueData;

    public void CreateRogue() {
        var startEvent = YiTeRogueEvent.GetStartEvent(rogueData.floor);
        var eventQueue = new Queue<YiTeRogueEvent>();
        var eventMap = new List<YiTeRogueEvent>(){ startEvent };

        eventQueue.Enqueue(startEvent);

        while (eventQueue.Count > 0) {
            var peekEvent = eventQueue.Dequeue();
            var nextEventList = YiTeRogueEvent.GetNextEventList(rogueData.floor, peekEvent)
                ?.Where(x => !eventMap.Exists(y => (x.pos == y.pos) && (x.step == y.step))).ToList();
            
            if (nextEventList == null)
                break;

            nextEventList.ForEach(eventQueue.Enqueue);
            eventMap.AddRange(nextEventList);
        }

        rogueData.eventMap = eventMap;
        SaveSystem.SaveData();
    }
}
