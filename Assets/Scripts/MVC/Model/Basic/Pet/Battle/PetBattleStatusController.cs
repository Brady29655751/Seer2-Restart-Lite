using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PetBattleStatusController
{   
    private Status _initStatus, _addStatus, _multStatus, _hiddenStatus;
    private Status _powerup, _maxPowerUp, _minPowerUp;

    private int _hp, _maxHp;
    private int _anger, _minAnger, _maxAnger;

    public Status initStatus => new Status(_initStatus);
    public Status addStatus => new Status(_addStatus);
    public Status multStatus => new Status(_multStatus);
    public Status currentStatus => GetCurrentStatus();
    public Status hiddenStatus => GetHiddenStatus();

    public Status powerup => Status.FloorToInt(Status.Clamp(_powerup, _minPowerUp, _maxPowerUp));
    public Status minPowerUp => new Status(_minPowerUp);
    public Status maxPowerUp => new Status(_maxPowerUp);

    public BattleStatus battleStatus => new BattleStatus(currentStatus, hiddenStatus);

    public int hp {
        get => Mathf.Clamp(_hp, 0, _maxHp);
        set => _hp = Mathf.Clamp(value, 0, _maxHp);
    }
    public int maxHp {
        get => Mathf.Max(_maxHp, 0);
        set => _maxHp = Mathf.Max(value, 0);
    }
    public int anger {
        get => Mathf.Clamp(_anger, _minAnger, _maxAnger);
        set => _anger = Mathf.Clamp(value, _minAnger, _maxAnger);
    }
    public int minAnger {
        get => Mathf.Max(_minAnger, 0);
        set => _minAnger = Mathf.Max(value, 0);
    }
    public int maxAnger {
        get => Mathf.Max(_maxAnger, 0);
        set => _maxAnger = Mathf.Max(value, 0);
    }

    public PetBattleStatusController(Status normalStatus, float initHp, int initAnger = 20) {
        _initStatus = new Status(normalStatus);
        _addStatus = Status.zero;
        _multStatus = Status.one;
        _hiddenStatus = new Status(0, 0, 0, 0, 100, 100);

        _powerup = new Status();
        _maxPowerUp = Status.FloorToInt(6 * Status.one);
        _minPowerUp = Status.FloorToInt(-6 * Status.one);

        _hp = (int)initHp;
        _maxHp = (int)normalStatus.hp;
        _anger = initAnger;
        _minAnger = 0;
        _maxAnger = 100;
    }

    public PetBattleStatusController(BattleStatus battleStatus) {
        _initStatus = battleStatus.GetBasicStatus();
        _addStatus = Status.zero;
        _multStatus = Status.one;
        _hiddenStatus = battleStatus.GetHiddenStatus();

        _powerup = new Status();
        _maxPowerUp = 6 * Status.one;
        _minPowerUp = -6 * Status.one;

        _hp = (int)battleStatus.hp;
        _maxHp = (int)battleStatus.hp;
        _anger = 20;
        _minAnger = 0;
        _maxAnger = 100;
    }

    public PetBattleStatusController(PetBattleStatusController rhs) {
        _initStatus = new Status(rhs._initStatus);
        _addStatus = new Status(rhs._addStatus);
        _multStatus = new Status(rhs._multStatus);
        _hiddenStatus = new Status(rhs._hiddenStatus);

        _powerup = new Status(rhs._powerup);
        _maxPowerUp = new Status(rhs._maxPowerUp);
        _minPowerUp = new Status(rhs._minPowerUp);

        _hp = rhs._hp;
        _maxHp = rhs._maxHp;
        _anger = rhs._anger;
        _minAnger = rhs._minAnger;
        _maxAnger = rhs._maxAnger;
    } 

    public Status GetCurrentStatus(bool ignorePowerup = false) {
        var _powerupStatus = ignorePowerup ? Status.GetPowerUpBuff(powerup.neg) : Status.GetPowerUpBuff(powerup);
        Status status = new Status(_initStatus * _powerupStatus * _multStatus + _addStatus);
        status.hp = hp;
        return status;
    }

    public Status GetHiddenStatus() {
        var status = new Status(_hiddenStatus);
        status[4] = Mathf.Max(status[4], 0);
        status[5] = Mathf.Max(status[5], 0);
        return status;
    }

    public int GetPowerUp(int type) {
        return (int)powerup[type];
    }

    public void SetPowerUp(int type, int powerup) {
        _powerup[type] = Mathf.Clamp(powerup, _minPowerUp[type], _maxPowerUp[type]);
    }

    public void SetPowerUp(Status powerup) {
        for (int i = 0; i < Status.typeNames.Length; i++) {
            SetPowerUp(i, (int)powerup[i]);
        }
    }

    public void SetMinPowerUp(int type, int min) {
        _minPowerUp[type] = min;
    }
    public void SetMinPowerUp(Status minPowerup) {
        for (int i = 0; i < Status.typeNames.Length; i++) {
            SetMinPowerUp(i, (int)minPowerup[i]);
        }
    }

    public void SetMaxPowerUp(int type, int max) {
        _maxPowerUp[type] = max;
    }

    public void SetMaxPowerUp(Status maxPowerup) {
        for (int i = 0; i < Status.typeNames.Length; i++) {
            SetMaxPowerUp(i, (int)maxPowerup[i]);
        }
    }

    public void AddPowerUp(int type, int addAmount) {
        int newPowerup = (int)powerup[type] + addAmount;
        SetPowerUp(type, newPowerup);
    }

    public void AddBattleStatus(int type, int addAmount) {
        int basicTypeLength = Status.typeNames.Length;
        if (type < basicTypeLength) {
            AddCurrentStatus(type, addAmount);
            return;
        }
        AddHiddenStatus(type - basicTypeLength, addAmount);
    }

    public void MultCurrentStatus(int type, float multAmount) {
        _multStatus[type] *= multAmount;
    }

    public void AddCurrentStatus(int type, int addAmount) {
        _addStatus[type] += addAmount;
    }

    public void AddHiddenStatus(int type, int addAmount) {
        _hiddenStatus[type] += addAmount;
    }
}
