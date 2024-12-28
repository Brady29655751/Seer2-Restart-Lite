using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectHandler
{

    private List<KeyValuePair<object, Effect>> queue = new List<KeyValuePair<object, Effect>>();

    public EffectHandler() {
        queue = new List<KeyValuePair<object, Effect>>();
    }

    public void Clear() {
        queue.Clear();
    }

    public EffectHandler Concat(EffectHandler rhs) {
        queue.AddRange(rhs.queue);
        return this;
    }

    public void AddEffects(object invokeUnit, List<Effect> effects) {
        if (effects == null)
            return;

        queue.AddRange(effects.Select(x => new KeyValuePair<object, Effect>(invokeUnit, x)));
    }

    public void RemoveEffects(Func<Effect, bool> filter) {
        queue.RemoveAll(pair => filter(pair.Value));
    }

    public List<Effect> GetEffects(Func<Effect, bool> filter) {
        return queue.Select(pair => pair.Value).Where(filter).ToList();
    }

    public List<Effect> GetEffects(Func<Effect, int, bool> filter) {
        return queue.Select(pair => pair.Value).Where(filter).ToList();
    }

    public List<bool> Condition(BattleState state, bool checkPhase = true) {
        if ((state != null) && (state.whosTurn == 0)) {
            var tempState = new BattleState(state);
            tempState.whosTurn = -1;
            var clientCheckList = Condition(tempState, checkPhase);
            tempState.whosTurn = 1;
            var masterCheckList = Condition(tempState, checkPhase);
            return masterCheckList.Select((x, i) => x || (clientCheckList[i])).ToList();
        }
        List<bool> checkList = new List<bool>();
        queue = queue.OrderBy(x => x.Value.priority).ToList();
        for (int i = 0; i < queue.Count; i++) {
            checkList.Add(queue[i].Value.Condition(queue[i].Key, state, checkPhase));
        }
        return checkList;
    }

    public void Apply(BattleState state, List<bool> checkList = null) {
        if (checkList.Count < queue.Count)
            return;
        
        for (int i = 0; i < queue.Count; i++) {
            if ((checkList == null) || (checkList[i])) {
                queue[i].Value.Apply(queue[i].Key, state);
            }
        }
    }

    public void CheckAndApply(BattleState state, bool checkPhase = true) {
        Apply(state, Condition(state, checkPhase));
    }
}
