using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Buff
{
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
    public List<Effect> effects = new List<Effect>();
    public Dictionary<string, object> options = new Dictionary<string, object>();


    public Buff(int _id, int _turn = -1, int _value = 0) {
        id = _id;
        turn = (_turn == -1) ? info.turn : _turn;
        value = _value;
        effects = info.effects.Select(x => new Effect(x)).ToList();

        foreach (var e in effects) {
            e.source = this;
        }
    }

    public Buff(Buff rhs) {
        id = rhs.id;
        turn = rhs.turn;
        value = rhs.value;
        effects = rhs.effects.Select(x => new Effect(x)).ToList();
        options = new Dictionary<string, object>(rhs.options);
        
        foreach (var e in effects) {
            e.source = this;
        }
    }

    public bool IsPower() {
        return id.IsWithin(1, 10);
    }
    public bool IsPowerUp() {
        for (int i = 1; i <= 9; i += 2) {
            if (id == i)
                return true;
        }
        return false;
    }
    public bool IsPowerDown() {
        for (int i = 2; i <= 10; i += 2) {
            if (id == i)
                return true;
        }
        return false;
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

        if (id.IsWithin(110, 116))
            return false;

        return IsAbnormal();
    }

    public static BuffInfo GetBuffInfo(int id) {
        return Database.instance.GetBuffInfo(id);
    }

    public static int GetPowerUpBuffId(int type, int powerup) {
        if (powerup == 0)
            return 0;

        return 2 * type + (powerup > 0 ? 1 : 2);
    }

    public static Buff GetWeatherBuff(int weatherId) {
        int id = 50_0000 + weatherId;
        if (GetBuffInfo(id) == null)
            return new Buff(50_0000);

        return new Buff(id);
    }

    public static Buff GetFeatureBuff(PetInfo info) {
        Buff featureBuff = new Buff((info.id > 0 ? 10_0000 : 90_0000) + Mathf.Abs(info.feature.baseId));
        return featureBuff;
    }

    public static Buff GetEmblemBuff(PetInfo info) {
        int id = (info.id > 0 ? 20_0000 : 80_0000) + Mathf.Abs(info.feature.baseId);
        if (GetBuffInfo(id) == null)
            return null;

        return new Buff(id, -1, 1);
    }

    public static string GetBuffDescriptionPreview(string rawDescription, string valueReplace = "0") {
        string desc = rawDescription;
        desc = desc.Replace("[value]", valueReplace).Replace("[ENDL]", "\n");
        desc = desc.Replace("[-]", "</color>").Replace("[", "<color=#").Replace("]", ">");
        return desc;
    }

    public string GetDescription() {
        string desc = GetBuffDescriptionPreview(info.description, value.ToString());

        if (!info.keep && !IsUnhealthy() && !IsAbnormal()) 
            desc += "（若精灵换场则不保留）";
        
        if (turn > 0)
            desc += "\n持续 " + turn + " 回合";
        
        return desc;
    }

    public float GetBuffIdentifier(string id) {
        return id switch {
            "type" => (float)info.type,
            "value" => value,
            "turn" => turn,
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
        }
    }

}
