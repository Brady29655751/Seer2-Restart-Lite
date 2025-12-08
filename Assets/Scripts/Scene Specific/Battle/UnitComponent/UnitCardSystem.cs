using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UnitCardSystem
{
    public const int MAX_HAND_COUNT = 8;
    public string[] placeIds => new string[] { "hand", "deck", "grave" };
    public List<Skill> hand, deck, grave;
    public Dictionary<string, string> options = new Dictionary<string, string>();

    public UnitCardSystem(BattlePet[] petBag)
    {   
        var pets = petBag.Where(x => x != null).Take(3);

        hand = new List<Skill>();
        deck = new List<Skill>();
        grave = new List<Skill>();

        foreach (var pet in pets)
        {
            hand.AddRange(pet.normalSkill.Where(x => !Skill.IsNullOrEmpty(x)).Select(x =>
            {
                var skill = new Skill(x);
                skill.SetSkillIdentifier("option[belongPet.skinId]", pet.ui.skinId);
                return skill;
            }));
        }

        hand = hand.Random(4, false);

        foreach (var pet in pets)
        {
            deck.AddRange(pet.ownSkill.Where(x => !Skill.IsNullOrEmpty(x))
                .Where(x => (x.positionType != SkillType.必杀) && !hand.Exists(y => x.id == y.id))
                .Select(x =>
            {
                var skill = new Skill(x);
                skill.SetSkillIdentifier("option[belongPet.skinId]", pet.ui.skinId);
                return skill;
            }));
        }

        // deck = Enumerable.Range(100001, 30).Select(x => new Skill(Skill.GetSkill(x), false)).ToList();
        // Draw(4);

        options = new Dictionary<string, string>();
    }

    public UnitCardSystem(UnitCardSystem rhs)
    {
        hand = rhs.hand?.ToList();
        deck = rhs.deck?.ToList();
        grave = rhs.grave?.ToList();
        options = new Dictionary<string, string>(rhs.options);
    }

    public List<Skill> GetCardsByPlace(string placeId)
    {
        return placeId switch
        {
            "hand"  =>  hand,
            "deck"  =>  deck,
            "grave" =>  grave,
            _       =>  null,
        };
    }

    public float GetCardIdentifier(string id)
    {
        if (id.TryTrimStart("option", out var trimId) && trimId.TryTrimParentheses(out trimId))
        {
            return Identifier.GetNumIdentifier(options.Get(trimId, "0"));
        }

        if (id.TryTrimStart("turn.", out _))
        {
            return GetCardIdentifier($"option[{id}]");
        }

        foreach (var place in placeIds)
        {
            if (id.TryTrimStart(place, out trimId))
            {
                IEnumerable<Skill> cards = GetCardsByPlace(place).Where(x => x != null);
                if (cards == null)
                    continue;

                while (trimId.TryTrimParentheses(out var trimExpr, '(', ')'))
                {
                    var op = "=";
                    var options = trimExpr.Split(':');
                    if (options.Length != 2)
                    {
                        var cond = Operator.SplitCondition(trimExpr, out op);
                        options = new string[] { cond.Key, cond.Value };
                    }

                    cards = cards.Where(x => Operator.Condition(op,
                        x.TryGetSkillIdentifier(options[0], out var num) ? num : Identifier.GetNumIdentifier(options[0]),
                        x.TryGetSkillIdentifier(options[1], out var num1) ? num1 : Identifier.GetNumIdentifier(options[1]))
                    );

                    trimId = trimId.TrimStart("(" + trimExpr + ")");
                }

                trimId = trimId.TrimStart(".");
                var split = trimId.Split('.');

                if (split.Length <= 1)
                    return split[0] switch
                    {
                        "count" => cards.Count(),
                        _ => cards.FirstOrDefault()?.GetSkillIdentifier(split[0]) ?? float.MinValue,
                    };

                return split[1] switch
                {
                    "sum" => cards.Sum(x => x.GetSkillIdentifier(split[0])),
                    "max" => cards.Max(x => x.GetSkillIdentifier(split[0])),
                    "min" => cards.Min(x => x.GetSkillIdentifier(split[0])),
                    _ => cards.FirstOrDefault()?.GetSkillIdentifier(split[0]) ?? float.MinValue,
                };
            }    
        }

        return id switch
        {
            _   =>   float.MinValue,
        };
    }

    public bool TryGetCardIdentifier(string id, out float value)
    {
        value = GetCardIdentifier(id);
        return value != float.MinValue;
    }

    public void SetCardIdentifier(string id, float value)
    {
        if (id.TryTrimStart("turn.", out _))
        {
            SetCardIdentifier($"option[{id}]", value);
            return;
        }

        if (id.TryTrimStart("option", out var trimId) && trimId.TryTrimParentheses(out trimId))
        {
            options.Set(trimId, value.ToString());
        }
    }

    public void OnRoundStart()
    {
        options = options.Where(x => !x.Key.StartsWith("turn.")).ToDictionary(x => x.Key, x => x.Value);

        Draw(4);
    }

    public void OnRoundEnd()
    {
        grave.AddRange(hand.Where(x => 
            (x.GetSkillIdentifier("option[keep]") == 0) &&
            (x.GetSkillIdentifier("option[token]") == 0)
        ));
        hand = hand.Where(x => x.GetSkillIdentifier("option[keep]") > 0).ToList();
    }

    public List<Skill> Draw(int count, Func<Skill, bool> filter = null)
    {
        var getCards = new List<Skill>();
        count = Mathf.Min(count, MAX_HAND_COUNT - hand.Count);
        if (count <= 0)
            return getCards;

        // 检索卡片只从现有的牌堆检索
        if (filter != null)
        {
            getCards = deck.Where(filter).ToList().Random(count, false);
            hand.AddRange(getCards);
            deck.RemoveRange(getCards);

            if (deck.Count == 0)
                Shuffle();

            return getCards;
        }

        // 抽取卡片若牌堆数不足则会中途洗牌
        var diff = count - deck.Count;
        if (diff > 0)
        {
            getCards.AddRange(Draw(deck.Count));
            Shuffle();
            getCards.AddRange(Draw(diff));

            if (deck.Count == 0)
                Shuffle();

            return getCards;
        }

        // 正常抽取卡片
        getCards = deck.Random(count, false);
        hand.AddRange(getCards);
        deck.RemoveRange(getCards);

        if (deck.Count == 0)
            Shuffle();

        return getCards;
    }

    public void Shuffle()
    {
        deck.AddRange(grave.Select(x => {
            var skill = new Skill(Skill.GetSkill(x.id));
            skill.SetSkillIdentifier("option[belongPet.skinId]", x.GetSkillIdentifier("option[belongPet.skinId]"));
            return skill;
        }));
        grave.Clear();
    }

    public void Use(int index)
    {
        if (!index.IsInRange(0, hand.Count))
            return;

        var skill = hand[index];
        hand.Remove(skill);

        if (skill.GetSkillIdentifier("option[token]") == 0)
            grave.Add(skill);

        SetCardIdentifier($"turn.usedCardCount", GetCardIdentifier($"turn.usedCardCount") + 1);
    }

    public void GetToken(List<Skill> tokens)
    {
        tokens.ForEach(x => x.SetSkillIdentifier("option[token]", 1));
        hand.AddRange(tokens.Take(MAX_HAND_COUNT - hand.Count));
    }

    public void Discard(int count, Func<Skill, bool> filter = null)
    {
        var discardCards = hand.Where(filter ?? (x => true)).ToList().Random(count, false);
        grave.AddRange(discardCards.Where(x => x.GetSkillIdentifier("option[token]") == 0));
        hand.RemoveRange(discardCards);
    }
}
