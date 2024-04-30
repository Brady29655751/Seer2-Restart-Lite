using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PetBattleBuffController
{
    private Element element;
    private List<int> blockBuffIds = new List<int>();
    private List<BuffType> blockBuffTypes = new List<BuffType>();

    public List<Buff> buffs = new List<Buff>();
    public bool isMovable => !IsUnmovableEffected();

    public PetBattleBuffController(Element element, List<Buff> buffs = null) {
        SetElement(element);
        AddRangeBuff(buffs, null, null);
    }

    public PetBattleBuffController(PetBattleBuffController rhs) {
        element = rhs.element;
        blockBuffIds = new List<int>(rhs.blockBuffIds);
        buffs = rhs.buffs.Select(x => new Buff(x)).ToList();
    }

    public void SetElement(Element newElement) {
        this.element = newElement;
        int elementBlockId = element switch {
            Element.水 => 1000,
            Element.火 => 1001,
            Element.草 => 1007,
            Element.风 => 1014,
            Element.冰 => 1002,
            _ => 0
        };
        blockBuffIds.Add(elementBlockId);
    }

    public bool IsUnmovableEffected() {
        return buffs.Any(x => x.IsUnmovable());
    }

    public bool IsBuffBlocked(int id) {
        var type = Buff.GetBuffInfo(id)?.type ?? BuffType.None;
        return blockBuffIds.Contains(id) || blockBuffTypes.Contains(type);
    }

    public Buff GetBuff(int id) {
        return buffs.Find(x => x.id == id);
    }

    public List<Buff> GetRangeBuff(Predicate<Buff> predicate) {
        return buffs.FindAll(predicate);
    }

    private void OnAddBuff(Buff newBuff, Unit buffUnit, BattleState state) {
        if (newBuff == null)
            return;

        var tmpPhase = state.phase;
        state.phase = EffectTiming.OnAddBuff;
        for (int i = 0; i < newBuff.effects.Count; i++) {
            newBuff.effects[i].CheckAndApply(buffUnit, state, true, false);
        }
        state.phase = tmpPhase;
    }

    private void NewBuff(Buff newBuff, Unit buffUnit, BattleState state) {
        if (newBuff == null)
            return;
            
        buffs.Add(newBuff);
        OnAddBuff(newBuff, buffUnit, state);
    }

    public void AddBuff(Buff newBuff, Unit buffUnit, BattleState state) {
        if (newBuff == null)
            return;

        if (blockBuffIds.Contains(newBuff.id) && !newBuff.IsPower())
            return;

        if (state == null) {
            buffs.Add(newBuff);
            return;
        }

        Buff oldBuff = GetBuff(newBuff.id);

        if (oldBuff == null) {
            NewBuff(newBuff, buffUnit, state);
            return;
        }

        switch (newBuff.info.copyHandleType) {
            default:
            case CopyHandleType.New:
                NewBuff(newBuff, buffUnit, state);
                break;
            case CopyHandleType.Block:
                break;
            case CopyHandleType.Replace:
                int oldBuffTurn = (oldBuff.turn == -1) ? int.MaxValue : oldBuff.turn;
                int newBuffTurn = (newBuff.turn == -1) ? int.MaxValue : newBuff.turn;
                if (oldBuffTurn <= newBuffTurn) {
                    RemoveBuff(oldBuff, buffUnit, state);
                    NewBuff(newBuff, buffUnit, state);
                }
                break;
            case CopyHandleType.Stack:
                if (oldBuff.value < oldBuff.info.maxValue) {
                    oldBuff.value += newBuff.value;
                    OnAddBuff(newBuff, buffUnit, state);
                }
                break;
            case CopyHandleType.Max:
                if (newBuff.value > oldBuff.value) {
                    RemoveBuff(oldBuff, buffUnit, state);
                    NewBuff(newBuff, buffUnit, state);
                }
                break;
            case CopyHandleType.Min:
                if (newBuff.value < oldBuff.value) {
                    RemoveBuff(oldBuff, buffUnit, state);
                    NewBuff(newBuff, buffUnit, state);
                }
                break;
        }
    }

    public void AddRangeBuff(IEnumerable<Buff> buffRange, Unit buffUnit, BattleState state) {
        if (buffRange == null)
            return;

        foreach (var buff in buffRange) {
            AddBuff(buff, buffUnit, state);
        }
    }

    private void OnRemoveBuff(Buff buff, Unit buffUnit, BattleState state) {
        if (buff == null)
            return;
        
        if (state == null)
            return;

        var tmpPhase = state.phase;
        state.phase = EffectTiming.OnRemoveBuff;
        for (int i = 0; i < buff.effects.Count; i++) {
            buff.effects[i].CheckAndApply(buffUnit, state, true, false);
        }
        state.phase = tmpPhase;
    }


    public void RemoveBuff(Buff buff, Unit buffUnit, BattleState state) {
        if (buff == null)
            return;

        OnRemoveBuff(buff, buffUnit, state);
        buffs.Remove(buff);
    }

    public void RemoveBuff(Predicate<Buff> pred, Unit buffUnit, BattleState state) {
        RemoveBuff(buffs.Find(pred), buffUnit, state);
    }

    public void RemoveRangeBuff(IEnumerable<Buff> buffRange, Unit buffUnit, BattleState state) {
        if (buffRange == null)
            return;
            
        foreach (var buff in buffRange) {
            RemoveBuff(buff, buffUnit, state);
        }
    }

    public void RemoveRangeBuff(Predicate<Buff> pred, Unit buffUnit, BattleState state) {
        RemoveRangeBuff(buffs.FindAll(pred), buffUnit, state);
    }

    public void BlockBuff(List<int> idList) {
        blockBuffIds.AddRange(idList);
    }

    public void BlockBuffWithType(List<BuffType> typeList) {
        blockBuffTypes.AddRange(typeList);
    }

    public void UnblockBuff(List<int> idList) {
        blockBuffIds.RemoveRange(idList);
    }

    public void UnblockBuffWithType(List<BuffType> typeList) {
        blockBuffTypes.RemoveRange(typeList);
    }

    public void OnTurnStart(Unit thisUnit, BattleState state) {
        if (state.turn == 1)
            return;
        
        foreach (var buff in buffs) {
            if (buff.turn > 0) {
                buff.turn--;
            }
        }
        RemoveRangeBuff(x => x.turn == 0, thisUnit, state);
    }

}
