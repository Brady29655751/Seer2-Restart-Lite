using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Buff
{
    public const int BUFFID_PVP_IV_120 = 61_0000;
    public const int BUFFID_PET_EXCHANGE = 62_0000;
    public const int BUFFID_PET_RANDOM = 62_0001;
    public static List<Buff> yiteEndBuffDatabse => BuffInfo.database.Where(x => x.id.IsInRange(410000, 420000)).Select(x => new Buff(x.id)).ToList();
    public static List<Buff> yiteHardBuffDatabse => BuffInfo.database.Where(x => x.id.IsInRange(430000, 440000) || x.id.IsInRange(400010, 400015)).Select(x => new Buff(x.id)).ToList();
    public static List<Buff> yiteEasyBuffDatabse => BuffInfo.database.Where(x => x.id.IsInRange(420000, 430000) || x.id.IsInRange(400005, 400010)).Select(x => new Buff(x.id)).ToList();

    /* Basic Data */
    public BuffInfo info => GetBuffInfo(id);
    public int id;
    public string name => info.name;
    public string description => GetDescription();
    public Sprite icon => info.icon;

    /* Battle Data */
    private int _value;
    public int value {
        get => Mathf.Clamp(_value, info.minValue, info.maxValue);
        set => _value = Mathf.Clamp(value, info.minValue, info.maxValue);
    }
    public int turn;
    public bool ignore;
    public List<Effect> effects = new List<Effect>();
    public Dictionary<string, string> options = new Dictionary<string, string>();


    public Buff(int _id, int _turn = -1, int _value = 0) {
        id = _id;
        turn = (_turn == -1) ? info.turn : _turn;
        value = _value;
        ignore = false;
        effects = info.effects.Select(x => new Effect(x)).ToList();

        foreach (var e in effects) {
            e.source = this;
        }
    }

    public Buff(Buff rhs) {
        id = rhs.id;
        turn = rhs.turn;
        value = rhs.value;
        ignore = rhs.ignore;
        effects = rhs.effects.Select(x => new Effect(x)).ToList();
        options = new Dictionary<string, string>(rhs.options);
        
        foreach (var e in effects) {
            e.source = this;
        }
    }

    public bool IsType(BuffType type) {
        return type switch {
            BuffType.All        => !IsPower(),
            BuffType.TurnBased  => (turn > 0) && (!IsUnhealthy()) && (!IsAbnormal()),
            _                   => info.type == type,
        };
    }

    public bool IsUneffectable() {
        return (id <= 0) && (!BuffInfo.IsMod(id));
    }

    public bool IsPower() {
        return id.IsWithin(1, 10) || id.IsWithin(-9, -10);
    }
    public bool IsPowerUp() {
        for (int i = 1; i <= 9; i += 2) {
            if (id == i)
                return true;
        }
        return id == -9;
    }
    public bool IsPowerDown() {
        for (int i = 2; i <= 10; i += 2) {
            if (id == i)
                return true;
        }
        return id == -10;
    }
    public bool IsAbnormal() {
        return info.type == BuffType.Abnormal;
    }
    public bool IsUnhealthy() {
        return info.type == BuffType.Unhealthy;
    }

    public bool IsUnmovable() {
        if (id == 104)
            return value < 40;

        return !info.movable;
    }
    public bool IsYiTeMedicineItem() {
        return id.IsInRange(400005, 400015);
    }

    public static BuffInfo GetBuffInfo(int id) {
        return Database.instance.GetBuffInfo(id);
    }

    public static int GetExtendUIBuffId(bool extend) {
        return extend ? -6 : -5;
    }

    public static Buff GetExtendUIBuff(bool extend) {
        return new Buff(GetExtendUIBuffId(extend));
    }

    public static int GetPowerUpBuffId(int type, int powerup) {
        if (powerup == 0)
            return 0;

        if (type == 5)
            return(powerup > 0) ? -9 : -10;

        return 2 * type + (powerup > 0 ? 1 : 2);
    }

    public static Buff GetWeatherBuff(int weatherId) {
        int id = 50_0000 + weatherId;
        if (GetBuffInfo(id) == null)
            return new Buff(50_0000);

        return new Buff(id);
    }

    public static Buff GetFeatureBuff(Pet pet) {
        var featureId = pet.feature.feature.baseId;
        Buff featureBuff = new Buff((featureId > 0 ? 10_0000 : 90_0000) + Mathf.Abs(featureId));
        return featureBuff;
    }

    public static Buff GetEmblemBuff(Pet pet) {
        var emblemId = pet.feature.emblem?.baseId ?? 0;
        int id = (emblemId >= 0 ? 20_0000 : 80_0000) + Mathf.Abs(emblemId);

        if (GetBuffInfo(id) == null)
            return null;

        return new Buff(id, -1, 1);
    }

    public static string GetBuffDescriptionPreview(string rawDescription, Buff referenceBuff = null) {
        string desc = rawDescription;

        referenceBuff ??= new Buff(-1, 0, 0);

        var pokerValue = referenceBuff.value switch {
            11  => "J",
            12  => "Q",
            13  => "K",
            1   => "A",
            _   => referenceBuff.value.ToString()
        };

        desc = desc.Replace("[option[value]]", referenceBuff.options.Get("value", "0"));
        desc = desc.Replace("[value]", referenceBuff.value.ToString()).Replace("[poker]", pokerValue);
        desc = desc.Replace("[ENDL]", "\n").Replace("[-]", "</color>").Replace("[", "<color=#").Replace("]", ">");
        return desc;
    }

    public string GetDescription() {
        string desc = GetBuffDescriptionPreview(info.description, this);

        if (!info.keep && !IsUnhealthy() && !IsAbnormal()) 
            desc += "（若精灵换场则不保留）";
        
        if (turn > 0)
            desc += "\n持续 " + turn + " 回合";
        
        return desc;
    }

    public float GetBuffIdentifier(string id) {
        if (id.TryTrimStart("option", out var trimId)) {
            trimId = trimId.TrimParentheses();
            return Identifier.GetNumIdentifier(options.Get(trimId, info.options.Get(trimId, "0")));
        }

        return id switch {
            "id" => this.id,
            "type" => (float)info.type,
            "value" => value,
            "turn" => turn,
            "ignore" => ignore ? 1 : 0,
            _ => float.MinValue,
        };
    }

    public bool TryGetBuffIdentifier(string id, out float num) {
        num = GetBuffIdentifier(id);
        return num != float.MinValue;
    }

    public void SetBuffIdentifier(string id, float num) {
        switch (id) {
            default:
                return;
            case "value":
                this.value = (int)num;
                return;
            case "turn":
                turn = (int)num;
                return;
            case "ignore":
                ignore = num != 0;
                return;
        }
    }

}
