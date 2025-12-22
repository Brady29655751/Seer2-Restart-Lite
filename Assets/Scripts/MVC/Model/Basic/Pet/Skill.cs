using System;
using System.Linq;
using System.Xml.Serialization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Skill
{
    public const int DATA_COL = 9;
    public static List<Skill> database => Database.instance?.skillDict.Values.Where(x => x != null).ToList() ?? new List<Skill>();

    public int id;
    public string name;
    public int elementId;
    public Element element
    {
        get => (Element)elementId;
        set => elementId = (int)value;
    }
    public SkillType type;
    public int power;

    public string rawCostString;
    public int anger, pp, maxPP;
    public int PP
    {
        get => Mathf.Clamp(pp, 0, maxPP);
        set => pp = Mathf.Clamp(value, 0, maxPP);
    }

    public int accuracy;
    public string rawDescription;
    public string description => GetDescription(rawDescription);
    public List<Effect> effects = new List<Effect>();
    public Dictionary<string, string> options = new Dictionary<string, string>();

    /* Hidden status */
    public bool isSecondSuper;
    public List<string> referBuffList = new List<string>();

    public float critical;
    public int combo, chain;
    public int priority;
    public bool ignoreShield = false;
    public bool ignorePowerup
    {
        get => powerup.mult.def * powerup.mult.mdf == 0;
        set => powerup.mult.def = powerup.mult.mdf = value ? 0 : 1;
    }
    public bool ignorePowerdown
    {
        get => powerdown.mult.atk * powerdown.mult.mat == 0;
        set => powerdown.mult.atk = powerdown.mult.mat = value ? 0 : 1;
    }
    public Linear<Status> powerup = new Linear<Status>(Status.one, Status.zero);
    public Linear<Status> powerdown = new Linear<Status>(Status.one, Status.zero);


    /* Properties */
    public SkillType positionType => GetPositionType();
    public bool isSuper => type == SkillType.必杀;
    public bool isAction => IsAction();
    public bool isAttack => IsAttack();
    public bool isSelect => IsSelect();
    public bool isCapture => IsCapture();

    public float animSpeed => float.TryParse(options.Get("anim_speed", "1"), out var speed) ? speed : 1;
    public string soundId => options.Get("sound", string.Empty);
    public PetAnimationType petAnimType => GetPetAnimationType();
    public PetAnimationType captureAnimType => GetCaptureAnimationType();

    public static bool IsMod(int id)
    {
        return id < -10000;
    }

    public static bool IsNullOrEmpty(Skill skill) 
    {
        return (skill == null) || (skill.type == SkillType.空过);
    }

    public Skill() { }

    public Skill(string[] _data, int startIndex = 0)
    {
        string[] _slicedData = new string[DATA_COL];
        Array.Copy(_data, startIndex, _slicedData, 0, _slicedData.Length);

        id = int.Parse(_slicedData[0]);
        name = _slicedData[1];
        elementId = int.Parse(_slicedData[2]);
        element = (Element)elementId;
        type = (SkillType)int.Parse(_slicedData[3]);
        power = int.Parse(_slicedData[4]);

        InitCost(_slicedData[5]);

        accuracy = int.Parse(_slicedData[6]);
        options.ParseOptions(_slicedData[7]);

        combo = chain = 1;
        InitOptionsProperty();

        rawDescription = _slicedData[8];
    }

    public Skill(int id, string name, Element element, SkillType type,
        int power, string cost, int accuracy, string options, string rawDescription)
    {
        this.id = id;
        this.name = name.ReplaceSpecialWhiteSpaceCharacters(string.Empty);
        this.element = element;
        this.type = type;
        this.power = power;

        InitCost(cost);

        this.accuracy = accuracy;
        this.options.ParseOptions(options.ReplaceSpecialWhiteSpaceCharacters(string.Empty));

        combo = chain = 1;
        InitOptionsProperty();

        this.rawDescription = rawDescription.ReplaceSpecialWhiteSpaceCharacters(string.Empty);
    }

    private void InitCost(string cost)
    {
        this.rawCostString = cost;

        var list = cost.ToIntList('/');
        anger = list[0];
        maxPP = (list.Get(1, 0) > 0) ? list[1] : Mathf.Max(5, 40 - anger);
        PP = maxPP;
    }

    private void InitOptionsProperty()
    {
        referBuffList = options.Get("ref_buff", null)?.Split('/').ToList();

        isSecondSuper = bool.Parse(options.Get("second_super", "false"));
        critical = float.Parse(options.Get("critical", "5"));
        priority = int.Parse(options.Get("priority", "0"));
        ignoreShield = bool.Parse(options.Get("ignore_shield", "false"));
        ignorePowerup = bool.Parse(options.Get("ignore_powerup", "false"));
        ignorePowerdown = bool.Parse(options.Get("ignore_powerdown", "false"));
    }

    public Skill(Skill rhs, bool copyPP = true)
    {
        id = rhs.id;
        name = rhs.name;
        elementId = rhs.elementId;
        element = rhs.element;
        type = rhs.type;
        power = rhs.power;
        anger = rhs.anger;
        maxPP = rhs.maxPP;
        PP = copyPP ? rhs.PP : maxPP;
        accuracy = rhs.accuracy;
        options = rhs.options.ToDictionary(entry => entry.Key, entry => entry.Value);
        rawDescription = rhs.rawDescription;
        SetEffects(rhs.effects.Select(x => new Effect(x)).ToList());

        isSecondSuper = rhs.isSecondSuper;
        critical = rhs.critical;
        combo = rhs.combo;
        chain = rhs.chain;
        priority = rhs.priority;
        ignoreShield = rhs.ignoreShield;
        powerup = new Linear<Status>(new Status(rhs.powerup.mult), new Status(rhs.powerup.add));
        powerdown = new Linear<Status>(new Status(rhs.powerdown.mult), new Status(rhs.powerdown.add));
        referBuffList = rhs.referBuffList?.ToList();
    }

    protected Skill(SkillType specialType)
    {
        id = (int)specialType;
        type = specialType;
        power = anger = 0;
        accuracy = 100;
        combo = chain = 1;
    }

    public string[] GetRawInfoStringArray()
    {
        string[] defaultOptionKeys = new string[] { "critical", "priority", "second_super",
            "ignore_shield", "ignore_powerup", "ignore_powerdown", "ref_buff" };

        string rawOptionString = ((critical == 5) ? string.Empty : ("critical=" + critical + "&")) +
            ((priority == 0) ? string.Empty : ("priority=" + priority + "&")) +
            (isSecondSuper ? ("second_super=true&") : string.Empty) +
            (ignoreShield ? ("ignore_shield=" + ignoreShield + "&") : string.Empty) +
            (ignorePowerup ? ("ignore_powerup=" + ignorePowerup + "&") : string.Empty) +
            (ignorePowerdown ? ("ignore_powerdown=" + ignorePowerdown + "&") : string.Empty) +
            (ListHelper.IsNullOrEmpty(referBuffList) ? string.Empty : ("ref_buff=" + referBuffList.ConcatToString("/")));

        string otherOptionString = options.Where(entry => !defaultOptionKeys.Contains(entry.Key)).Select(entry => entry.Key + "=" + entry.Value).ConcatToString("&");
        string allOptionString = string.IsNullOrEmpty(rawOptionString + otherOptionString) ? "none" : (rawOptionString + "&" + otherOptionString);

        return new string[] { id.ToString(), name, elementId.ToString(), ((int)type).ToString(),
            power.ToString(), rawCostString, accuracy.ToString(), allOptionString.Trim('&'), rawDescription  };
    }

    public static Skill GetSkill(int id, bool avoidNull = true)
    {
        Skill skill = Database.instance.GetSkill(id);
        return avoidNull ? (skill ?? GetNoOpSkill()) : skill;
    }

    public override string ToString()
    {
        return "id: " + id + " name: " + name;
    }

    public static Skill ParseRPCData(string[] data)
    {
        var len = data.Length;
        int id = int.Parse(data[0]);
        var skill = id switch
        {
            (int)SkillType.空过 => Skill.GetNoOpSkill(),
            (int)SkillType.道具 => Skill.GetItemSkill(new Item(int.Parse(data[1]))),
            (int)SkillType.換场 => Skill.GetPetChangeSkill(int.Parse(data[1]), int.Parse(data[2]), bool.Parse(data[3])),
            (int)SkillType.逃跑 => Skill.GetEscapeSkill(),
            _ => Skill.GetSkill(id)
        };

        if (!skill.isAction)
        {
            var selectEffects = skill.effects.Where(x => x.isSelect).ToList();
            var targetIndexList = data[1].Split('/');
            for (int i = 0; i < selectEffects.Count; i++)
                selectEffects[i].abilityOptionDict.Set("target_index", targetIndexList.Get(i, "-1"));

            skill.options.Set("evolve", data[2]);
        }

        var parallelSourceIndex = int.TryParse(data.Get(len - 2, "0"), out var num) ? num : 0;
        var parallelTargetIndex = int.TryParse(data.Get(len - 1, "0"), out num) ? num : 0;
        skill.SetParallelIndex(parallelSourceIndex, parallelTargetIndex);

        return skill;
    }

    public string[] ToRPCData(BattleSettings settings)
    {
        var data = id switch
        {
            (int)SkillType.空过 => new string[] { id.ToString() },
            (int)SkillType.道具 => new string[] { id.ToString(), options.Get("item_id", "0") },
            (int)SkillType.換场 => new string[] { id.ToString(), options.Get("source_index", "0"), options.Get("target_index", "0"), options.Get("passive", "false") },
            (int)SkillType.逃跑 => new string[] { id.ToString() },
            _ => new string[] { id.ToString(), effects.Where(x => x.isSelect).Select(e => e.abilityOptionDict.Get("target_index", "-1")).ConcatToString("/"), options.Get("evolve", "0") }
        };

        if (settings.parallelCount > 1)
            data.Append(options.Get("parallel_source_index", "0")).Append(options.Get("parallel_target_index", "0")).ToArray();

        return data;
    }

    public static Skill GetRandomSkill()
    {
        var skillData = GameManager.versionData.skillData;
        int minSkillId = skillData.minSkillId;
        int maxSkillId = skillData.maxSkillId;
        while (true)
        {
            int skillId = Random.Range(minSkillId, maxSkillId + 1);
            Skill skill = Skill.GetSkill(skillId);
            if ((skill.type != SkillType.必杀) && (!skill.isSelect))
                return skill;
        }
    }

    public static Skill GetNoOpSkill()
    {
        Skill skill = new Skill(SkillType.空过);
        skill.name = "空过";
        skill.rawDescription = "跳过自己的回合";
        skill.critical = 5;
        skill.accuracy = 100;
        return skill;
    }

    public static Skill GetEscapeSkill()
    {
        Skill skill = new Skill(SkillType.逃跑);
        skill.name = "逃跑";
        skill.priority = 6;
        skill.SetEffects(Effect.GetEscapeEffect());
        return skill;
    }

    public static Skill GetPetChangeSkill(int sourceIndex, int targetIndex, bool passive = false)
    {
        Skill skill = new Skill(SkillType.換场);
        skill.name = "換场";
        skill.options.Set("source_index", sourceIndex.ToString());
        skill.options.Set("target_index", targetIndex.ToString());
        skill.options.Set("passive", passive.ToString());
        skill.SetEffects(Effect.GetPetChangeEffect(sourceIndex, targetIndex, passive));
        return skill;
    }

    public static Skill GetPetChangeSkillWithTiming(Skill petChangeSkill, EffectTiming timing)
    {
        if (petChangeSkill == null)
            return null;

        if (petChangeSkill.type != SkillType.換场)
            return petChangeSkill;

        var skill = new Skill(petChangeSkill);
        var effectList = skill.effects.Where(x => (x != null) && (x.ability == EffectAbility.PetChange)).ToList();
        foreach (var e in effectList)
            e.SetTiming(timing);

        return skill;
    }

    public static Skill GetItemSkill(Item item)
    {
        Skill skill = new Skill(SkillType.道具);
        skill.name = $"【道具】{item.name}";
        skill.options.Set("item_id", item.id.ToString());
        skill.effects = item.effects;
        return skill;
    }

    public static string GetSkillDescriptionPreview(string plainText)
    {
        return plainText.Replace("[ENDL]", "\n").Replace("[-]", "</color>").Replace("[", "<color=#").Replace("]", ">");
    }

    public string GetDescription(string plainText, string mode = "skill")
    {
        string desc;
        desc = plainText.Trim();

        if ((critical <= 100) && (critical != 5))
        {
            desc = $"[ff50d0]【暴击率 {critical}%】[-][ENDL]{desc}";
        }

        if ((accuracy <= 100) && (accuracy != (isAttack ? 95 : 100)))
        {
            desc = $"[52e5f9]【命中率 {accuracy}%】[-][ENDL]{desc}";
        }

        if (priority != 0)
        {
            var priDesc = "[77e20c]【先制" + ((priority > 0) ? "+" : string.Empty) + priority + "】[-][ENDL]";
            desc = priDesc + desc;
        }

        if (mode == "card")
        {
            desc = GetAdditionalCardDescription(desc);
        }

        if (!ListHelper.IsNullOrEmpty(referBuffList))
        {
            for (int i = 0; i < referBuffList.Count; i++)
            {
                var buffValueList = referBuffList[i].TrimParenthesesLoop();
                bool isWithValue = !ListHelper.IsNullOrEmpty(buffValueList);
                var buffId = int.Parse(isWithValue ? referBuffList[i].Substring(0, referBuffList[i].IndexOf('[')) : referBuffList[i]);
                var buffValue = isWithValue ? int.Parse(buffValueList.FirstOrDefault(x => !x.Contains(":")) ?? "0") : 0;
                var buff = new Buff(buffId, -2, buffValue);

                buffValueList?.RemoveAll(x => !x.Contains(":"));
                buffValueList?.ForEach(x => buff.options.Set(x.Split(':')[0], x.Split(':')[1]));

                desc += ("[ENDL][ENDL][ffbb33]【" + buff.name + "】[-]：" + buff.description);
            }
        }
        return Skill.GetSkillDescriptionPreview(desc);
    }

    public string GetAdditionalCardDescription(string desc, bool withBasicInfo = true)
    {
        var spAbility = new List<string>(){ "token", "keep" };

        for (int id = -101; id > -200; id--)
        {
            var buffInfo = Buff.GetBuffInfo(id);
            if (buffInfo == null)
                break;

            if (GetSkillIdentifier($"option[{spAbility.Get(-id - 101)}]") != 0)
            {
                desc = $"[ff8cff]【{buffInfo.name}】[-][ENDL]{desc}";       
                referBuffList = referBuffList?.Union(id.ToString().SingleToList()).ToList() ?? new List<string>(){ id.ToString() };
            }
        }

        if (!withBasicInfo)
            return desc;

        var elementColor = PetElementSystem.elementColorList.Get((int)element, "#ffffff").TrimStart('#');
        desc = $"<size=4>[ENDL]</size><size=16>[{elementColor}]{element.GetElementName()}系[-][ffbb33]【{type}】[-]威力 {power}</size>[ENDL][ENDL]{desc}";

        return desc;
    }

    public void SetEffects(Effect _effect)
    {
        _effect.source = this;
        effects = new List<Effect>() { _effect };
    }

    public void SetEffects(List<Effect> _effects)
    {
        effects = Effect.SetEffects(_effects);
        foreach (var e in effects)
            e.source = this;
    }

    public void SetParallelIndex(int parallelSourceIndex = 0, int parallelTargetIndex = 0)
    {
        options.Set("parallel_source_index", parallelSourceIndex.ToString());
        options.Set("parallel_target_index", parallelTargetIndex.ToString());

        effects.ForEach(x =>
        {
            x?.abilityOptionDict?.Set("parallel_source_index", parallelSourceIndex.ToString());
            x?.abilityOptionDict?.Set("parallel_target_index", parallelTargetIndex.ToString());
        });
    }

    public bool IsAction()
    {
        return (type < SkillType.属性) && (type != SkillType.被动);
        // (type != SkillType.属性) && (type != SkillType.物理) 
        // && (type != SkillType.特殊) && (type != SkillType.必杀);
    }

    public bool IsAttack()
    {
        return type > SkillType.属性;
        // (type == SkillType.物理) || (type == SkillType.特殊) || (type == SkillType.必杀);
    }

    public bool IsSelect()
    {
        return effects.Exists(x => x.IsSelect());
    }

    public bool IsSelectReady()
    {
        return effects.All(x => (!x.IsSelect()) ||
            (x.abilityOptionDict.Get("target_index", "-1") != "-1"));
    }

    public List<bool> GetSelectableTarget(BattlePet[] petBag, int cursor, int parallelCount)
    {
        var result = new List<bool>();
        var isSelectDead = effects.Exists(x => x.targetType.Contains("dead"));
        var isSelectOther = effects.Exists(x => x.targetType.Contains("other"));
        for (int i = 0; i < petBag.Length; i++) {  
            var index = i; 
            var isTarget = (petBag[index] != null) && (isSelectDead ^ (!petBag[index].isDead));
            var isCursor = isSelectOther && (cursor == index);
            var isParallel = (parallelCount <= 1) || (i < parallelCount);
            var interactable = isTarget && (!isCursor) && isParallel;
            result.Add(interactable);
        }
        return result;
    }

    public bool IsCapture()
    {
        return effects.Any(x => x.ability == EffectAbility.Capture);
    }

    public SkillType GetPositionType()
    {
        if (options.TryGet("position", out var value))
            return (value.ToLower() == "super") ? SkillType.必杀 : SkillType.连携;

        return type;
    }

    public PetAnimationType GetPetAnimationType()
    {
        if (type == SkillType.物理)
            return PetAnimationType.Physic;

        if (type == SkillType.特殊)
            return PetAnimationType.Special;

        if (type == SkillType.属性)
            return PetAnimationType.Property;

        if ((type == SkillType.必杀) || (type == SkillType.连携))
            return isSecondSuper ? PetAnimationType.SecondSuper : PetAnimationType.Super;

        return PetAnimationType.None;
    }

    public PetAnimationType GetCaptureAnimationType()
    {
        if (isCapture)
            return (options.Get("capture_result", "false") == "false") ?
                PetAnimationType.CaptureFail : PetAnimationType.CaptureSuccess;

        return PetAnimationType.None;
    }

    public float GetSkillIdentifier(string id)
    {
        if (id.TryTrimStart("option", out var trimId))
            return Identifier.GetNumIdentifier(options.Get(trimId.TrimParentheses(), "0"));

        if (id.TryTrimStart("powerup.", out trimId))
        {
            var split = trimId.Split('.');
            var power = split[0] switch
            {
                "pos" => powerup,
                "neg" => powerdown,
                _ => new Linear<Status>(Status.one, Status.zero),
            };
            var status = split[1] switch
            {
                "mult" => power.mult,
                "add" => power.add,
                _ => Status.zero,
            };
            return status[split[2]];
        }

        if (id.TryTrimStart("effect.", out trimId))
        {
            if (trimId.TryTrimStart("ability", out var trimExpr))
            {
                var trimAbility = trimExpr.TrimParenthesesLoop('(', ')');
                var okEffects = new List<Effect>(effects);

                for (int i = 0; i < trimAbility.Count; i++)
                {
                    var split = trimAbility[i].Split(':');
                    if (split.Length == 1)
                        okEffects.RemoveAll(x => x.ability != split[0].ToEffectAbility());
                    else
                        okEffects.RemoveAll(x => x.abilityOptionDict.Get(split[0]) != split[1]);

                    trimExpr = trimExpr.TrimStart('(' + trimAbility[i] + ')');
                }

                trimExpr = trimExpr.TrimStart('.');
                return trimExpr switch
                {
                    "count" => okEffects.Count,
                    _ => okEffects.Count,
                };
            }
        }

        return id switch
        {
            "id" => this.id,
            "element" => elementId,
            "type" => (float)type,
            "power" => power,
            "anger" => anger,
            "pp" => PP,
            "maxPP" => maxPP,
            "lostPP" => maxPP - PP,
            "accuracy" => accuracy,
            "priority" => priority,
            "critical" => critical,
            "combo" => combo,
            "chain" => chain,
            "ignoreShield" => ignoreShield ? 1 : 0,
            "ignorePowerup" => ignorePowerup ? 1 : 0,
            "ignorePowerdown" => ignorePowerdown ? 1 : 0,
            "effect" => effects.Count,
            "parallelTargetIndex" => GetSkillIdentifier("option[parallel_target_index]"),
            "isSelect" => IsSelect() ? 1 : 0,
            "evolve" => GetSkillIdentifier("option[evolve]"),
            _ => float.MinValue,
        };
    }

    public bool TryGetSkillIdentifier(string id, out float num)
    {
        num = GetSkillIdentifier(id);
        return num != float.MinValue;
    }

    public void SetSkillIdentifier(string id, float value)
    {
        if (id.TryTrimStart("option", out var trimId) && trimId.TryTrimParentheses(out var optionKey))
        {
            options.Set(optionKey, value.ToString());
            return;
        }
        
        if (id.TryTrimStart("powerup.", out trimId))
        {
            var split = trimId.Split('.');
            var power = split[0] switch
            {
                "pos" => powerup,
                "neg" => powerdown,
                _ => new Linear<Status>(Status.one, Status.zero),
            };
            var status = split[1] switch
            {
                "mult" => power.mult,
                "add" => power.add,
                _ => Status.zero,
            };
            status[split[2]] = value;
            return;
        }

        switch (id)
        {
            default:
                return;
            case "element":
                elementId = (int)value;
                element = (Element)elementId;
                return;
            case "type":
                type = (SkillType)value;
                return;
            case "power":
                power = Mathf.Max((int)value, 0);
                return;
            case "anger":
                anger = Mathf.Max((int)value, 0);
                return;
            case "pp":
                PP = (int)value;
                return;
            case "maxPP":
                maxPP = Mathf.Max((int)value, 0);
                return;
            case "accuracy":
                accuracy = (int)value;
                return;
            case "priority":
                priority = (int)value;
                return;
            case "critical":
                critical = value;
                return;
            case "combo":
                combo = Mathf.Max((int)value, 0);
                return;
            case "chain":
                chain = (int)value;
                return;
            case "ignoreShield":
                ignoreShield = (value != 0);
                return;
            case "ignorePowerup":
                ignorePowerup = (value != 0);
                return;
            case "ignorePowerdown":
                ignorePowerdown = (value != 0);
                return;
            case "effect":
                SetEffects((value == 0) ? new List<Effect>() : (Skill.GetSkill((int)value, false)?.effects.Select(x => new Effect(x)).ToList() ?? new List<Effect>()));
                return;
            case "evolve":
                SetSkillIdentifier("option[evolve]", value);
                return;
            case "parallelTargetIndex":
                SetParallelIndex(int.Parse(options.Get("parallel_source_index", "0")), (int)value);
                return;
        }
    }

}

public enum SkillType
{
    空过 = -1, 道具 = -2, 換场 = -3, 逃跑 = -4, 被动 = -5,
    属性 = 0, 物理 = 1, 特殊 = 2, 连携 = 3, 必杀 = 100,
}

