using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class PetBattleBuffController
{
    public Element element { get; private set; }
    public Element subElement { get; private set; }

    private Dictionary<string, int> blockBuffSps = new Dictionary<string, int>();
    private List<int> blockBuffIds = new List<int>();
    private List<int> copyBuffIds = new List<int>();
    private List<BuffType> blockBuffTypes = new List<BuffType>();
    private List<BuffType> copyBuffTypes = new List<BuffType>();

    public List<Buff> buffs = new List<Buff>();
    public bool isMovable => !IsUnmovableEffected();

    public PetBattleBuffController(Element element, Element subElement = Element.普通, List<Buff> buffs = null) {
        SetElement(element);
        SetSubElement(subElement);
        AddRangeBuff(buffs, null, null);
    }

    public PetBattleBuffController(PetBattleBuffController rhs) {
        element = rhs.element;
        subElement = rhs.subElement;
        blockBuffSps = new Dictionary<string, int>(rhs.blockBuffSps);
        blockBuffIds = new List<int>(rhs.blockBuffIds);
        copyBuffIds = new List<int>(rhs.copyBuffIds);
        blockBuffTypes = new List<BuffType>(rhs.blockBuffTypes);
        copyBuffTypes = new List<BuffType>(rhs.copyBuffTypes);
        buffs = rhs.buffs.Select(x => new Buff(x)).ToList();
    }

    private int GetElementBlockId(Element element) {
        return element switch {
            Element.水 => 1000,
            Element.火 => 1001,
            Element.草 => 1007,
            Element.风 => 1014,
            Element.冰 => 1002,
            _ => 0
        };
    }

    public void SetElement(Element newElement) {
        blockBuffIds.Remove(GetElementBlockId(element));
        blockBuffIds.Add(GetElementBlockId(newElement));

        this.element = newElement;
    }

    public void SetSubElement(Element newElement) {
        this.subElement = newElement;
    }

    public bool IsUnmovableEffected() {
        return buffs.Exists(x => x.IsUnmovable());
    }

    public bool IsBuffBlocked(Buff buff) {
        if (buff == null)
            return false;

        return IsBuffIdBlocked(buff.id) || blockBuffTypes.Exists(buff.IsType);
    }

    public bool IsBuffIdBlocked(int id) {
        return blockBuffIds.Contains(id);
    }

    public bool IsBuffTypeBlocked(BuffType type) {
        return blockBuffTypes.Contains(type);
    }

    public bool IsBuffCopied(Buff buff) {
        if (buff == null)
            return false;

        return IsBuffIdCopied(buff.id) || copyBuffTypes.Exists(buff.IsType);
    }

    public bool IsBuffIdCopied(int id) {
        return copyBuffIds.Contains(id);
    }

    public bool IsBuffTypeCopied(BuffType type) {
        return copyBuffTypes.Contains(type);
    }

    public Buff GetBuff(int id) {
        return buffs.Find(x => x.id == id);
    }

    public List<Buff> GetRangeBuff(Predicate<Buff> predicate) {
        return buffs.FindAll(predicate);
    }

    public void OnAddBuff(Buff newBuff, Unit buffUnit, BattleState state) {
        if (newBuff == null)
            return;

        var tmpPhase = state.phase;
        state.phase = EffectTiming.OnAddBuff;
        
        newBuff.effects.OrderBy(x => x.priority).ToList()
            .ForEach(x => x.CheckAndApply(buffUnit, state, true, false));
        state.phase = tmpPhase;
    }

    private void NewBuff(Buff newBuff, Unit buffUnit, BattleState state) {
        if (newBuff == null)
            return;
            
        buffs.Add(newBuff);
        OnAddBuff(newBuff, buffUnit, state);
    }

    public bool AddBuff(Buff newBuff, Unit buffUnit, Unit invokeUnit, BattleState state, bool triggerCopy = true) {
        if (newBuff == null)
            return false;

        // 判斷反饋
        if (triggerCopy && IsBuffCopied(newBuff) && newBuff.IsCopyable()) {
            var rhsUnit = state?.GetRhsUnitById(buffUnit.id);
            rhsUnit?.pet.buffController.AddBuff(newBuff, rhsUnit, buffUnit, state, false);
        }

        // 判斷特殊免疫
        if ((invokeUnit != null) && (invokeUnit.id != buffUnit.id) && (blockBuffSps.Get("op", 0) > 0))
            return false;

        // 判斷正常免疫
        if (IsBuffBlocked(newBuff) && !newBuff.IsPower())
            return false;

        bool isResisted = false;

        // 判斷抗性
        var resistBuff = GetBuff(Buff.BUFFID_RESIST_BUFF);
        if (resistBuff != null)
        {
            var buffType = newBuff.info.type.ToRawString();
            if (resistBuff.TryGetBuffIdentifier($"option[{buffType}Id]", out var resistId) && 
                (resistId == newBuff.id) && (Random.Range(0, 100) < resistBuff.GetBuffIdentifier($"option[{buffType}Value]")))
            {
                var newId = newBuff.info.type switch
                {
                    BuffType.Unhealthy => 1999, 
                    BuffType.Abnormal => 199,
                    _ => newBuff.id,  
                };

                newBuff.SetBuffIdentifier("id", newId);  
                newBuff.SetBuffIdentifier("turn", 1); 
                newBuff.SetBuffIdentifier("value", resistId); 
                isResisted = true;
            }
        }

        // 判斷是否處於戰鬥時
        if (state == null) {
            buffs.Add(newBuff);
            return !isResisted;
        }

        // 判斷是否重複
        Buff oldBuff = GetBuff(newBuff.id);

        // 添加印記
        if (oldBuff == null) {
            NewBuff(newBuff, buffUnit, state);
            return !isResisted;
        }

        switch (newBuff.info.copyHandleType) {
            default:
            case CopyHandleType.New:
                NewBuff(newBuff, buffUnit, state);
                return !isResisted;
            case CopyHandleType.Block:
                return false;
            case CopyHandleType.Replace:
                int oldBuffTurn = (oldBuff.turn == -1) ? int.MaxValue : oldBuff.turn;
                int newBuffTurn = (newBuff.turn == -1) ? int.MaxValue : newBuff.turn;
                if (oldBuffTurn <= newBuffTurn) {
                    RemoveBuff(oldBuff, buffUnit, state);
                    NewBuff(newBuff, buffUnit, state);
                    return !isResisted;
                }
                return false;
            case CopyHandleType.Stack:
                if (oldBuff.value < oldBuff.info.maxValue) {
                    oldBuff.value += newBuff.value;
                    OnAddBuff(newBuff, buffUnit, state);
                    return !isResisted;
                }
                return false;
            case CopyHandleType.Max:
                if (newBuff.value > oldBuff.value) {
                    RemoveBuff(oldBuff, buffUnit, state);
                    NewBuff(newBuff, buffUnit, state);
                    return !isResisted;
                }
                return false;
            case CopyHandleType.Min:
                if (newBuff.value < oldBuff.value) {
                    RemoveBuff(oldBuff, buffUnit, state);
                    NewBuff(newBuff, buffUnit, state);
                    return !isResisted;
                }
                return false;
        }
    }

    public bool AddRangeBuff(IEnumerable<Buff> buffRange, Unit buffUnit, BattleState state) {
        if (buffRange == null)
            return false;

        buffRange = buffRange.OrderBy(x => x?.info.addPriority ?? int.MinValue);

        var isSuccess = false;
        foreach (var buff in buffRange)
            isSuccess |= AddBuff(buff, buffUnit, buffUnit, state);

        return isSuccess;
    }

    public bool AddRangeBuff(IEnumerable<Buff> buffRange, Unit buffUnit, Unit invokeUnit, BattleState state) {
        if (buffRange == null)
            return false;

        buffRange = buffRange.OrderBy(x => x?.info.addPriority ?? int.MinValue);

        var isSuccess = false;
        foreach (var buff in buffRange)
            isSuccess |= AddBuff(buff, buffUnit, invokeUnit, state);

        return isSuccess;
    }

    public void OnRemoveBuff(Buff buff, Unit buffUnit, BattleState state) {
        if (buff == null)
            return;
        
        if (state == null)
            return;

        var tmpPhase = state.phase;
        state.phase = EffectTiming.OnRemoveBuff;
        buff.effects.OrderBy(x => x.priority).ToList()
            .ForEach(x => x.CheckAndApply(buffUnit, state, true, false));
        state.phase = tmpPhase;
    }


    public bool RemoveBuff(Buff buff, Unit buffUnit, BattleState state) {
        if (buff == null)
            return false;

        OnRemoveBuff(buff, buffUnit, state);
        return buffs.Remove(buff);
    }

    public bool RemoveBuff(Predicate<Buff> pred, Unit buffUnit, BattleState state) {
        return RemoveBuff(buffs.Find(pred), buffUnit, state);
    }

    public bool RemoveRangeBuff(IEnumerable<Buff> buffRange, Unit buffUnit, BattleState state) {
        if (buffRange == null)
            return false;
            
        var isSuccess = false;
        foreach (var buff in buffRange)
            isSuccess |= RemoveBuff(buff, buffUnit, state);

        return isSuccess;
    }

    public bool RemoveRangeBuff(Predicate<Buff> pred, Unit buffUnit, BattleState state, int randomCount = -1) {
        var count = randomCount < 0 ? int.MaxValue : randomCount;
        return RemoveRangeBuff(buffs.FindAll(pred)?.Random(count, false), buffUnit, state);
    }

    public void BlockBuff(List<int> idList) {
        blockBuffIds.AddRange(idList);
    }

    public void BlockBuffWithType(List<BuffType> typeList) {
        blockBuffTypes.AddRange(typeList);
    }

    public void BlockBuffWithString(string str)
    {
        if (string.IsNullOrEmpty(str))
            return;

        switch (str)
        {
            default:
                blockBuffSps.Set(str, blockBuffSps.Get(str, 0) + 1);
                break;
        }
    }

    public void UnblockBuff(List<int> idList) {
        blockBuffIds.RemoveRange(idList);
    }

    public void UnblockBuffWithType(List<BuffType> typeList) {
        blockBuffTypes.RemoveRange(typeList);
    }

    public void UnblockBuffWithString(string str)
    {
        if (string.IsNullOrEmpty(str))
            return;

        switch (str)
        {
            case "op":
                blockBuffSps.Set(str, blockBuffSps.Get(str, 0) - 1);
                break;
        }
    }

    public void CopyBuff(List<int> idList) {
        copyBuffIds.AddRange(idList);
    }

    public void CopyBuffWithType(List<BuffType> typeList) {
        copyBuffTypes.AddRange(typeList);
    }

    public void UncopyBuff(List<int> idList) {
        copyBuffIds.RemoveRange(idList);
    }

    public void UncopyBuffWithType(List<BuffType> typeList) {
        copyBuffTypes.RemoveRange(typeList);
    }

    public void ReduceBuffTurn(Unit thisUnit, BattleState state, string turnIdentifier = "turn") {
        if (state.turn == 1)
            return;
        
        var candidates = new List<Buff>();
        foreach (var buff in buffs) {
            var turn = buff.GetBuffIdentifier(turnIdentifier);
            if (turn > 0) {
                buff.SetBuffIdentifier(turnIdentifier, turn - 1);
                candidates.Add(buff);
            }
        }
        RemoveRangeBuff(x => candidates.Contains(x) && (x.GetBuffIdentifier(turnIdentifier) == 0), thisUnit, state);
    }

}
