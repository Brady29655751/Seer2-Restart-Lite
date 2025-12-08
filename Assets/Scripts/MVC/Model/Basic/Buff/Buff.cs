using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;

public class Buff
{
    public const int BUFFID_TOKEN = -21;
    public const int BUFFID_PVP_IV_120 = 61_0000;
    public const int BUFFID_PET_EXCHANGE = 62_0000;
    public const int BUFFID_PET_RANDOM = 62_0001;
    public const int BUFFID_PROTECT_POWERUP = -2003;
    public const int BUFFID_PROTECT_POWERDOWN = -2004;
    public static List<int> powerupBuffIds => new List<int>() { 1, 3, 5, 7, 9, -9 };
    public static List<int> powerdownBuffIds => new List<int>(){ 2, 4, 6, 8, 10, -10 }; 
    public static List<Buff> yiteEndBuffDatabse => BuffInfo.database.Where(x => x.id.IsInRange(410000, 420000)).Select(x => new Buff(x.id)).ToList();
    public static List<Buff> yiteHardBuffDatabse => BuffInfo.database.Where(x => x.id.IsInRange(430000, 440000) || x.id.IsInRange(400010, 400015) || x.id.IsInRange(950, 999)).Select(x => new Buff(x.id)).ToList();
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
    public bool hide => options.TryGet("hide", out var value) ? (float.Parse(value) != 0) : info.hide;
    public bool removable => options.TryGet("removable", out var value) ? (float.Parse(value) != 0) : info.removable;
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
            BuffType.Power      => IsPower(),
            BuffType.Powerup    => IsPowerUp(),
            BuffType.Powerdown  => IsPowerDown(),
            BuffType.TurnBased  => (turn > 0) && (!IsUnhealthy()) && (!IsAbnormal()),
            _                   => info.type == type,
        };
    }

    public bool IsUneffectable() {
        int[] idList = new int[]{ 90, 91, 99, 100 };
        return ((id <= 0) && (!BuffInfo.IsMod(id))) || idList.Contains(id) || (!removable);
    }

    public bool IsPower() {
        return id.IsWithin(1, 10) || id.IsWithin(-9, -10);
    }
    public bool IsPowerUp() {
        return powerupBuffIds.Contains(id);
    }
    public bool IsPowerDown() {
        return powerdownBuffIds.Contains(id);
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

    public static int GetPowerUpBuffId(int type, int powerup)
    {
        if (powerup == 0)
            return 0;

        if (type == 5)
            return (powerup > 0) ? -9 : -10;

        return 2 * type + (powerup > 0 ? 1 : 2);
    }

    public static int GetProtectBuffTypeId(BuffType type)
    {
        return type switch
        {
            BuffType.TurnBased => -2001,
            BuffType.ChanceBased => -2002,
            BuffType.Powerup => -2003,
            BuffType.Powerdown => -2004,
            _ => 0,
        };
    }

    public static Buff GetWeatherBuff(int weatherId)
    {
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

        string GetPokerValue(int value)
        {
            return value switch
            {
                11  => "J",
                12  => "Q",
                13  => "K",
                1   => "A",
                _   => value.ToString()
            };
        };

        string ReplaceValue(string str, string key)
        {
            var split = key.Split(':');
            var type = "value";
            var value = "value";

            type = split[0];

            if (split.Length == 2)
                value = split[1];

            if (type.TryTrimStart("option", out var optionName)
                && optionName.TryTrimParentheses(out optionName)
                && (!referenceBuff.options.ContainsKey(optionName)))
            {
                return str.Replace($"[{key}]", "空栏位");
            }

            var buffValue = (int)referenceBuff.GetBuffIdentifier(type);
            var valueName = value switch
            {
                "pet"   => Pet.GetPetInfo(buffValue)?.name ?? "精灵信息错误",
                "skill" => Skill.GetSkill(buffValue, false)?.name ?? "技能信息错误",
                "buff"  => Buff.GetBuffInfo(buffValue)?.name ?? "印记信息错误",
                "poker" => GetPokerValue(buffValue),
                _       => buffValue.ToString(),  
            };
            return str.Replace($"[{key}]", valueName.ToString());
        }

        // Match [xxx:yyy] or [option[xxx]:yyy] or [option[xxx]]
        Regex regex = new Regex(@"\[(?:option\[(?<key>[^\]]+)\](?::(?<value>[^\]]+))?|(?<left>[^:\]]+):(?<right>[^\]]+))\]");
        var matches = regex.Matches(desc).GroupBy(m => m.Value).Select(g => g.First()).ToList();

        foreach (Match match in matches)
        {
            desc = ReplaceValue(desc, match.Value.TrimParentheses());
        }

        /*
        int index = 0;
        while (true)
        {
            index = desc.IndexOf("[option[", index);
            if (index < 0)
                break;
            var optionKey = desc.Substring(index + 1).TrimParentheses();
            desc = ReplaceValue(desc, $"option[{optionKey}]");
            // desc = desc.Replace($"[option[{optionKey}]]", referenceBuff.options.Get(optionKey, $"空栏位"));
        }
        */
        
        desc = desc.Replace("[pet]", Pet.GetPetInfo(referenceBuff.value)?.name ?? "精灵信息错误");
        desc = desc.Replace("[skill]", Skill.GetSkill(referenceBuff.value, false)?.name ?? "技能信息错误");
        desc = desc.Replace("[buff]", Buff.GetBuffInfo(referenceBuff.value)?.name ?? "印记信息错误");
        desc = desc.Replace("[poker]", GetPokerValue(referenceBuff.value));
        desc = desc.Replace("[value]", referenceBuff.value.ToString());
        desc = desc.Replace("[ENDL]", "\n").Replace("[-]", "</color>").Replace("[", "<color=#").Replace("]", ">");
        return desc;
    }

    public string GetDescription() {
        string desc = GetBuffDescriptionPreview(info.description, this);

        if (!info.keep && !IsUnhealthy() && !IsAbnormal())
            desc += "\n（换场不保留原精灵身上）";

        if (info.inherit)
            desc += "\n（换场继承给下一位精灵）";
            
        if (info.legacy)
            desc += "\n（阵亡继承给下一位精灵）";
        
        if (turn > 0)
            desc += $"\n持续 {turn} 回合";

        var round = GetBuffIdentifier("round");
        if (round > 0)
            desc += $"\n持续 {round} 轮次";
        
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
            "round" => GetBuffIdentifier("option[round]"),
            "ignore" => ignore ? 1 : 0,
            "hide" => hide ? 1 : 0,
            "removable" => removable ? 1 : 0,
            _ => float.MinValue,
        };
    }

    public bool TryGetBuffIdentifier(string id, out float num) {
        num = GetBuffIdentifier(id);
        return num != float.MinValue;
    }

    public void SetBuffIdentifier(string id, float num) {
        if (id.TryTrimStart("option", out var trimId) && trimId.TryTrimParentheses(out trimId))
        {
            options.Set(trimId, num.ToString());
            return;
        }
        switch (id)
        {
            default:
                return;
            case "value":
                this.value = (int)num;
                return;
            case "turn":
                turn = (int)num;
                return;
            case "round":
                SetBuffIdentifier("option[round]", num);
                return;
            case "ignore":
                ignore = num != 0;
                return;
            case "hide":
            case "removable":
                SetBuffIdentifier($"option[{id}]", num);
                return;
        }
    }

}
