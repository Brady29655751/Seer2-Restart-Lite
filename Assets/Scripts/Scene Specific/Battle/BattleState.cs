using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System;

public class BattleState
{
    public BattleState lastTurnState = null;

    public int turn;
    public int parallelTurn;
    public int whosTurn;    // masterTurn: 1, clientTurn: -1
    public EffectTiming phase;
    public int weather;
    public Buff weatherBuff => Buff.GetWeatherBuff(weather);
    public List<KeyValuePair<string, Buff>> stateBuffs = new List<KeyValuePair<string, Buff>>();

    public Unit masterUnit, clientUnit;
    public List<int> actionOrder = new List<int>();
    public int actionCursor;

    public BattleSettings settings;
    public BattleResult result;

    public bool isAllUnitDone => IsAllUnitDone();
    public bool isAllUnitReady => IsAllUnitReady();
    public bool isAnyPetDead => IsAnyPetDead();
    public Unit myUnit => (settings.mode == BattleMode.PVP) ? (PhotonNetwork.IsMasterClient ? masterUnit : clientUnit) : masterUnit;
    public Unit opUnit => (settings.mode == BattleMode.PVP) ? (PhotonNetwork.IsMasterClient ? clientUnit : masterUnit) : clientUnit;
    public Unit atkUnit => (whosTurn == 0) ? null : GetUnitById(whosTurn);
    public Unit defUnit => (whosTurn == 0) ? null : GetUnitById(-whosTurn);

    public BattleState(BattleSettings settings, Unit masterTurn, Unit clientTurn) {
        this.lastTurnState = null;

        this.turn = 0;
        this.whosTurn = 0;
        this.phase = EffectTiming.OnBattleStart;
        this.weather = settings.weather;
        this.stateBuffs = settings.initBuffs;

        this.masterUnit = new Unit(masterTurn);
        this.clientUnit = new Unit(clientTurn);
        this.actionOrder = new List<int>();
        this.actionCursor = -1;

        this.settings = new BattleSettings(settings);
        this.result = new BattleResult(settings);
    }

    public BattleState(BattleState rhs) {
        lastTurnState = (rhs.lastTurnState == null) ? null : new BattleState(rhs.lastTurnState);

        turn = rhs.turn;
        parallelTurn = rhs.parallelTurn;
        whosTurn = rhs.whosTurn;
        phase = rhs.phase;
        weather = rhs.weather;
        stateBuffs = rhs.stateBuffs.Select(x => new KeyValuePair<string, Buff>(x.Key, new Buff(x.Value))).ToList();

        masterUnit = new Unit(rhs.masterUnit);
        clientUnit = new Unit(rhs.clientUnit);
        actionOrder = rhs.actionOrder.ToList();
        actionCursor = rhs.actionCursor;

        settings = new BattleSettings(rhs.settings);
        result = new BattleResult(rhs.result);
    }

    public virtual float GetStateIdentifier(string id) {
        string trimId = id;
        if (id.TryTrimStart("last.", out trimId))
            return (lastTurnState == null) ? float.MinValue : lastTurnState.GetStateIdentifier(trimId);

        if (id.TryTrimStart("settings.", out trimId))
            return settings.GetSettingsIdentifier(trimId);

        if (id.TryTrimStart("result.", out trimId))
            return result.GetResultIdentifier(trimId);

        if (id.TryTrimStart("buff", out trimId) &&
            trimId.TryTrimParentheses(out var buffKey)) {
            var buff = stateBuffs.Find(x => x.Key == buffKey).Value;
            if (buff == null)
                return float.MinValue;

            var buffExpr = trimId.TrimStart("[" + buffKey + "].");
            return buff.TryGetBuffIdentifier(buffExpr, out float num) ? num : Identifier.GetNumIdentifier(buffExpr);
        }

        return trimId switch {
            "turn" => turn,
            "actionCursor" => actionCursor,
            "phase" => (float)phase,
            "weather" => (float)weather,
            "whosTurn" => whosTurn,
            _ => float.MinValue,
        };
    }

    public virtual void OnTurnStart() {
        if (turn > 0) {
            if (lastTurnState != null) {
                lastTurnState.lastTurnState = null;
            }
            lastTurnState = new BattleState(this){ phase = EffectTiming.OnTurnEnd };
        }

        turn++;
        whosTurn = 0;
        actionOrder.Clear();
        actionCursor = -1;

        for (int i = 0; i < stateBuffs.Count; i++) {
            if (stateBuffs[i].Value.turn > 0)
                stateBuffs[i].Value.turn--;
        }

        stateBuffs.RemoveAll(x => x.Value.turn == 0);

        masterUnit.OnTurnStart(this);
        clientUnit.OnTurnStart(this);
    }

    public virtual bool IsAllUnitDone() {
        return actionCursor >= actionOrder.Count; 
        // masterUnit.isDone && clientUnit.isDone;
    }

    public virtual bool IsAllUnitReady() {
        return masterUnit.isReady && clientUnit.isReady;
    }

    public virtual bool IsAnyPetDead() {
        return masterUnit.pet.isDead || clientUnit.pet.isDead;
    }

    public virtual void GiveTurnToNextUnit() {
        actionCursor++;
        if (actionCursor >= actionOrder.Count) {
            whosTurn = 0;
            return;
        }

        whosTurn = (actionOrder[actionCursor] > 0) ? 1 : -1;

        var actionUnit = GetUnitById(whosTurn);
        actionUnit.isDone = false;

        if (settings.parallelCount > 1) {
            actionUnit.petSystem.cursor = Mathf.Abs(actionOrder[actionCursor]) - 1;
            GetRhsUnitById(actionUnit.id).petSystem.cursor = int.Parse(actionUnit.skill?.options.Get("parallel_target_index") ?? "0");
        }

        /*
        if (!masterUnit.isDone) {
            whosTurn = masterUnit.id;
            return;
        }

        if (!clientUnit.isDone) {
            whosTurn = clientUnit.id;
            return;
        }

        parallelTurn++;
        if ((settings.parallelCount > 1) && (parallelTurn < settings.parallelCount)) {
            masterUnit.isDone = clientUnit.isDone = false;
            masterUnit.petSystem.cursor = masterUnit.petSystem.GetNextCursorCircular();
            clientUnit.petSystem.cursor = clientUnit.petSystem.GetNextCursorCircular();
            whosTurn = actionOrder.FirstOrDefault();
            return;
        }

        whosTurn = 0;
        */
    }

    public virtual Unit GetUnitById(int id) {
        if (id > 0)
            return masterUnit;

        if (id < 0)
            return clientUnit;

        return null;
    }

    public virtual Unit GetRhsUnitById(int lhsId) {
        return GetUnitById(-lhsId);
    }

    public virtual bool IsGoFirst(Unit lhsUnit) {
        int unitId = lhsUnit.id;
        if (settings.parallelCount <= 1)
            return actionOrder.FirstOrDefault() == unitId;

        Unit rhsUnit = GetRhsUnitById(unitId);
        var lhsCursor = actionOrder.IndexOf((int)Mathf.Sign(lhsUnit.id) * (lhsUnit.petSystem.cursor + 1));
        var rhsCursor = actionOrder.IndexOf((int)Mathf.Sign(rhsUnit.id) * (rhsUnit.petSystem.cursor + 1));
        
        lhsCursor = (lhsCursor < 0) ? int.MaxValue : lhsCursor;
        rhsCursor = (rhsCursor < 0) ? int.MaxValue : rhsCursor;
        return lhsCursor < rhsCursor;
    }

    public virtual bool IsGoLast(Unit lhsUnit) {
        return !IsGoFirst(lhsUnit);
    }

    public virtual void ApplySkillsAndBuffs() {
        GetEffectHandler(null).CheckAndApply(this);
    }

    public virtual void ApplyBuffs() {
        GetEffectHandler(null, false).CheckAndApply(this);
    }

    public virtual EffectHandler GetEffectHandler(Unit invokeUnit, bool addSkillEffect = true) {

        if (invokeUnit == null)
            return GetEffectHandler(masterUnit).Concat(GetEffectHandler(clientUnit));

        var buffEffects = invokeUnit.pet.buffs.Where(x => !x.ignore).Select(x => x.effects);
        var handler = new EffectHandler();

        handler.AddEffects(invokeUnit, weatherBuff.effects);

        var stateEffects = stateBuffs.Where(x => !x.Value.ignore).Select(x => x.Value.effects);
        foreach (var e in stateEffects)
            handler.AddEffects(invokeUnit, e);

        if (addSkillEffect && (invokeUnit.skill != null))
            handler.AddEffects(invokeUnit, invokeUnit.skill.effects);

        foreach (var e in buffEffects)
            handler.AddEffects(invokeUnit, e);

        return handler;
    }

    public virtual List<Effect> GetEffectsByTiming(EffectTiming timing, EffectHandler handler, Func<Effect, bool> filter = null) {
        filter ??= (e) => true;

        var tmpPhase = phase;
        phase = timing;

        var condition = handler.Condition(this);
        var effects = handler.GetEffects((x, i) => condition[i] && filter(x));

        phase = tmpPhase;
        return effects;
    }
}