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
    public int whosTurn;    // masterTurn: 1, clientTurn: -1
    public EffectTiming phase;
    public Weather weather;
    public Unit masterUnit, clientUnit;
    public List<int> actionOrder = new List<int>();

    public BattleSettings settings;
    public BattleResult result;

    public bool isAllTurnDone => IsAllTurnDone();
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
        this.masterUnit = new Unit(masterTurn);
        this.clientUnit = new Unit(clientTurn);
        this.actionOrder = new List<int>();

        this.settings = new BattleSettings(settings);
        this.result = new BattleResult();
    }

    public BattleState(BattleState rhs) {
        lastTurnState = (rhs.lastTurnState == null) ? null : new BattleState(rhs.lastTurnState);

        turn = rhs.turn;
        whosTurn = rhs.whosTurn;
        phase = rhs.phase;
        weather = rhs.weather;
        masterUnit = new Unit(rhs.masterUnit);
        clientUnit = new Unit(rhs.clientUnit);
        actionOrder = rhs.actionOrder.ToList();

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

        return trimId switch {
            "turn" => turn,
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
        masterUnit.OnTurnStart(this);
        clientUnit.OnTurnStart(this);
    }

    public virtual bool IsAllTurnDone() {
        return masterUnit.isDone && clientUnit.isDone;
    }

    public virtual bool IsAnyPetDead() {
        return masterUnit.pet.isDead || clientUnit.pet.isDead;
    }

    public virtual void GiveTurnToNextUnit() {
        whosTurn = isAllTurnDone ? 0 : -whosTurn;
    }

    public virtual Unit GetUnitById(int id) {
        if (masterUnit.id == id)
            return masterUnit;

        if (clientUnit.id == id)
            return clientUnit;

        return null;
    }

    public virtual Unit GetRhsUnitById(int lhsId) {
        return GetUnitById(-lhsId);
    }

    public virtual bool IsGoFirst(int unitId) {
        return actionOrder.FirstOrDefault() == unitId;
    }

    public virtual bool IsGoLast(int unitId) {
        return actionOrder.LastOrDefault() == unitId;
    }
}