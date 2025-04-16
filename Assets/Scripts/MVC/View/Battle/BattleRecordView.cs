using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using BattleRecordAction = IKeyValuePair<string[], bool>;

//TODO Support Prev, Next, and Stop
public class BattleRecordView : Module
{
    private List<BattleRecordAction> actionList => Player.instance.currentBattleRecord.actionList;

    private int currentInfoIndex = -1;
    private List<BattleRecordTurnInfo> turnInfos = new List<BattleRecordTurnInfo>();
    private int totalSteps => turnInfos.Sum(x => x.steps);
    private int currentSteps => turnInfos.Take(currentInfoIndex + 1).Sum(x => x.steps);
    public BattleRecordTurnInfo currentTurnInfo => turnInfos.ElementAtOrDefault(currentInfoIndex);

    public void PrevStep() {

    }

    public void NextStep() {
        
    }

}

public class BattleRecordTurnInfo {
    public BattleState lastState, currentState;
    public int steps;
}
