using System;
using System.Linq;
using System.Xml.Serialization;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using Random = UnityEngine.Random;

public class BattleSettings
{
    public int seed = 0;
    [XmlAttribute("mod")] public bool isMod = false;
    [XmlAttribute("simulate")] public bool isSimulate = false;
    [XmlAttribute("escape")] public bool isEscapeOK = true;
    [XmlAttribute("capture")] public bool isCaptureOK = false;
    [XmlAttribute("item")] public bool isItemOK = true;
    [XmlElement("captureLevel")] public int captureLevel = 1;
    [XmlIgnore] public bool isPVP => GetSettingsIdentifier("mode") == (float)BattleMode.PVP;
    [XmlIgnore] public bool isAutoOK => (!isPVP) && (parallelCount <= 1) && (mode != BattleMode.Card);
    [XmlIgnore] public bool isReveal => (mode != BattleMode.PVP) || ((bool)PhotonNetwork.CurrentRoom.CustomProperties["reveal"]);
    [XmlIgnore] public bool isLimited => (starLimit > 0) || !string.IsNullOrEmpty(filterLimit);


    [XmlAttribute("count")] public int petCount = 6;
    [XmlAttribute("parallel")] public int parallelCount = 1;
    [XmlAttribute("star")] public int starLimit = 0;
    [XmlElement("limit")] public string filterLimit;
    [XmlElement("limitDesc")] public string filterLimitDesc;

    [XmlAttribute("time")] public int time = 10;
    [XmlElement("mode")] public int modeId = 0;
    [XmlIgnore]
    public BattleMode mode
    {
        get => (BattleMode)modeId;
        set => modeId = (int)value;
    }
    [XmlElement("rule")] public int ruleId = 0;
    [XmlIgnore]
    public BattleRule rule
    {
        get => (BattleRule)ruleId;
        set => ruleId = (int)value;
    }
    [XmlElement("weather")] public int weather = 0;
    [XmlElement("initBuff")] public string initBuffExpr;
    [XmlIgnore]
    public List<KeyValuePair<string, Buff>> initBuffs
    {
        get => GetInitBuffs();
        set => initBuffExpr = value.Select(x => "(" + x.Key + ":" + (x.Value?.id ?? 0) + ")").ConcatToString(string.Empty);
    }

    public BattleSettings()
    {
        seed = (int)DateTime.Now.Ticks;
    }

    public BattleSettings(BattleMode _mode, int _petCount = 6, int _weather = 0, bool _isEscapeOK = true, bool _isSimulate = false, bool _isCapture = false, bool _isItem = true, bool _isMod = false)
    {
        seed = (int)DateTime.Now.Ticks;

        isMod = _isMod;
        isSimulate = _isSimulate;
        isEscapeOK = _isEscapeOK;
        isCaptureOK = _isCapture;
        isItemOK = _isItem;

        mode = _mode;
        weather = _weather;
        petCount = _petCount;
    }

    public BattleSettings(BattleSettings rhs)
    {
        seed = rhs.seed;

        isMod = rhs.isMod;
        isSimulate = rhs.isSimulate;
        isEscapeOK = rhs.isEscapeOK;
        isCaptureOK = rhs.isCaptureOK;
        isItemOK = rhs.isItemOK;

        mode = rhs.mode;
        rule = rhs.rule;
        weather = rhs.weather;
        petCount = rhs.petCount;
        parallelCount = rhs.parallelCount;
        starLimit = rhs.starLimit;
        filterLimit = rhs.filterLimit;
        filterLimitDesc = rhs.filterLimitDesc;
        time = rhs.time;
        initBuffExpr = rhs.initBuffExpr;
    }

    public float GetSettingsIdentifier(string id)
    {
        return id switch
        {
            "mode" => (mode == BattleMode.Record) ? (int)BattleMode.PVP : modeId,
            "rule" => ruleId,
            "petCount" => petCount,
            "parallelCount" => parallelCount,
            "starLimit" => starLimit,
            "time" => time,
            "weather" => weather,
            "mod" => isMod ? 1 : 0,
            "simulate" => isSimulate ? 1 : 0,
            "escape" => isEscapeOK ? 1 : 0,
            "item" => isItemOK ? 1 : 0,
            "capture" => isCaptureOK ? 1 : 0,
            "captureLevel" => isCaptureOK ? captureLevel : -1,
            "reveal" => isReveal ? 1 : 0,
            _ => float.MinValue,
        };
    }

    public List<KeyValuePair<string, Buff>> GetInitBuffs()
    {
        return initBuffExpr?.TrimParenthesesLoop('(', ')')?.Select(x =>
        {
            var split = x.Split(':');
            return new KeyValuePair<string, Buff>(split[0], new Buff(int.Parse(split[1])));
        }).ToList() ?? new List<KeyValuePair<string, Buff>>();
    }

    public Func<Pet, bool> GetBattleLimit()
    {
        return Parser.ParseConditionFilter<Pet>(filterLimit, (x, pet) => 
            pet.TryGetPetIdentifier(x, out var num) ? num : Identifier.GetIdentifier(x));
    }

    public bool Condition(Pet pet, out string message)
    {
        message = null;
        if (isSimulate || (pet == null))
            return true;

        if ((!isMod) && isLimited && PetInfo.IsMod(pet.id))
        {
            message = $"无法使用Mod精灵进行本体关卡的挑战哦\n<color=#ffbb33>橘色名字</color>的是Mod精灵";
            return false;
        }

        if ((starLimit > 0) && (pet.info.star > starLimit))
        {
            message = $"不能携带超过<color=#ffbb33>{starLimit}星</color>的精灵进行挑战哦";
            return false;
        }

        message = filterLimitDesc;
        return GetBattleLimit()(pet);
    }

    public bool Condition(IEnumerable<Pet> pets, out string message)
    {
        foreach (var pet in pets)
        {
            if (!Condition(pet, out message))
                return false;
        }

        message = null;
        return true;
    }

    public BattleSettings FixToYiTeRogue()
    {
        mode = BattleMode.YiTeRogue;
        isSimulate = false;
        isItemOK = true;
        petCount = 6;
        parallelCount = 1;
        return this;
    }
}

public enum BattleMode
{
    Record = -1,
    Normal = 0,
    SelfSimulation = 1,
    PVP = 2,
    SPT = 3,
    Special = 4,
    YiTeRogue = 5,
    Card = 6,
}

public enum BattleRule
{
    Anger = 0,
    PP = 1,
}
