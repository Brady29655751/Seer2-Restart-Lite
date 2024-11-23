using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public static class Identifier {
    public static float GetNumIdentifier(string id) {
        string trimId;
        float num = 0;

        if (id.TryTrimStart("random", out trimId)) {
            if (trimId == string.Empty)
                return Random.Range(0, 100);

            if (trimId == "[old]")
                return Player.instance.random;

            if (trimId == "[new]") {
                Player.instance.random = Random.Range(0, 100);
                return Player.instance.random;
            }

            return trimId.ToIntRange().Random();
        }

        return float.TryParse(id, out num) ? num : 0;
    }

    public static float GetIdentifier(string id) {
        id = id.Replace("－", "-");

        string trimId = id;

        if (id.TryTrimStart("data.", out trimId)) {
            var dotIndex = trimId.IndexOf('.');
            var key = (dotIndex < 0) ? trimId : trimId.Substring(0, dotIndex);
            var data = Player.GetSceneData(key, 0);
            return data switch {
                Pet pet => pet.GetPetIdentifier(trimId.Substring(dotIndex + 1)),
                _ => GetNumIdentifier(data.ToString()),
            };
        }   
        
        if (id.TryTrimStart("firstPet.", out trimId))
            return GetIdentifier("petBag[0]." + trimId);

        if (id.TryTrimStart("petBag", out trimId) && trimId.TryTrimParentheses(out var indexExpr)) {
            var index = (int)GetIdentifier(indexExpr);
            return Player.instance.petBag[index].GetPetIdentifier(trimId.TrimStart("[" + indexExpr + "]."));
        }

        if (id.TryTrimStart("activity", out trimId)) {
            string activityId = trimId.TrimParentheses();
            
            trimId = trimId.Substring(trimId.IndexOf('.') + 1);
            
            if (!trimId.Contains("[default]"))
                trimId += "[default]0";

            string dataId = trimId.Substring(0, trimId.IndexOf("[default]"));
            string defaultVal = trimId.Substring(trimId.IndexOf("[default]") + 9);
            
            var activity = Activity.Find(activityId);
            var dataVal = activity.GetData(dataId, defaultVal);
            return float.TryParse(dataVal, out var num) ? num : 0;
        }

        if (id.TryTrimStart("battle.", out trimId)) {
            var battle = Player.instance.currentBattle;
            return GetBattleIdentifier("state." + trimId, battle?.currentState?.myUnit, battle?.currentState?.opUnit);
        }

        if (id.TryTrimStart("item", out trimId)) {
            int itemId = int.Parse(trimId.TrimParentheses());
            string dataId = trimId.Substring(trimId.IndexOf('.') + 1);
            Item item = Item.Find(itemId);

            return dataId switch {
                _ => item?.num ?? 0,
            };
        }

        return GetNumIdentifier(id);
    }

    public static float GetIdentifier(string id, Effect effect, Unit lhsUnit, Unit rhsUnit, object otherSource = null) {
        
        id = id.Replace("－", "-");

        float num = 0;
        bool isOtherSourceAvailable = (otherSource != null) && (id.TryTrimStart("target.", out id));
        object idSource = isOtherSourceAvailable ? otherSource : effect.source;
        
        return idSource switch {
            BattlePet battlePet => isOtherSourceAvailable ? GetPetIdentifier(id, lhsUnit.petSystem, battlePet) : 
                (battlePet.TryGetPetIdentifier(id, out num) ? num : GetBattleIdentifier(id, lhsUnit, rhsUnit)),
            Pet pet => pet.TryGetPetIdentifier(id, out num) ? num : GetBattleIdentifier(id, lhsUnit, rhsUnit),
            Buff buff => buff.TryGetBuffIdentifier(id, out num) ? num : GetBattleIdentifier(id, lhsUnit, rhsUnit),
            Skill skill => skill.TryGetSkillIdentifier(id, out num) ? num : GetBattleIdentifier(id, lhsUnit, rhsUnit),
            _ => GetBattleIdentifier(id, lhsUnit, rhsUnit),
        };
    }

    public static float GetBattleIdentifier(string id, Unit lhsUnit, Unit rhsUnit) {
        string trimId = id;
            
        if (id.TryTrimStart("state.", out trimId)) {
            var battle = Player.instance.currentBattle;
            var state = battle.currentState;

            if (trimId.StartsWith("last.")) {
                state = state.lastTurnState;
                if (state == null)
                    return 0;
            }

            var actionOrder = state.actionOrder;

            if (trimId.TryTrimStart("me.", out trimId)) {
                return trimId switch {
                    "order" => (ListHelper.IsNullOrEmpty(actionOrder)) ? 0 : (actionOrder.FirstOrDefault() == lhsUnit.id) ? 1 : 2,
                    _ => GetUnitIdentifier(trimId, lhsUnit)
                };
            }

            if (trimId.TryTrimStart("op.", out trimId)) {
                return trimId switch {
                    "order" => (ListHelper.IsNullOrEmpty(actionOrder)) ? 0 : (actionOrder.FirstOrDefault() == rhsUnit.id) ? 1 : 2,
                    _ => GetUnitIdentifier(trimId, rhsUnit)
                };
            }

            return battle.GetBattleIdentifier(trimId);
        }
            
        if (id.TryTrimStart("me.", out trimId))
            return GetUnitIdentifier(trimId, lhsUnit);

        if (id.TryTrimStart("op.", out trimId))
            return GetUnitIdentifier(trimId, rhsUnit);

        if (id.TryTrimStart("random", out trimId)) {
            if (trimId == string.Empty)
                return lhsUnit.random;

            if (trimId == "[new]")  
                return lhsUnit.RNG();
        }

        return GetNumIdentifier(id);        
    }

    public static float GetUnitIdentifier(string id, Unit unit) {
        string trimId = id;
        if (id.TryTrimStart("pet.", out trimId))
            return GetPetIdentifier(trimId, unit.petSystem);

        if (id.TryTrimStart("skill.", out trimId))
            return GetSkillIdentifier(trimId, unit.skillSystem);

        return GetNumIdentifier(id);
    }

    public static float GetPetIdentifier(string id, UnitPetSystem petSystem, BattlePet otherSource = null) {
        string trimId = id;

        if (petSystem.TryGetPetSystemIdentifier(id, out var num))
            return num;

        var pet = otherSource ?? petSystem.pet;

        if (id.TryTrimStart("bag", out trimId)) {
            IEnumerable<BattlePet> petBag = petSystem.petBag.Where(x => x != null);
            while (trimId.TryTrimParentheses(out var trimExpr, '(', ')')) {
                var op = "=";
                var options = trimExpr.Split(':');
                if (options.Length != 2) {
                    var split = Operator.SplitCondition(trimExpr, out op);
                    options = new string[] { split.Key, split.Value };  
                }
                petBag = petBag.Where(x => Operator.Condition(op, 
                    Identifier.GetPetIdentifier(options[0], petSystem, x),
                    Identifier.GetPetIdentifier(options[1], petSystem, x))
                );
                trimId.TrimStart("(" + trimExpr + ")");
            }

            trimId = trimId.TrimStart(".");
            var count = petBag.Count();

            if (trimId == "count")
                return count;

            if (count <= 0)
                return float.MinValue;

            pet = petBag.First();
        }

        if (id == "movable")
            return pet.isMovable ? 1 : 0;

        if (id == "dead")
            return pet.isDead ? 1 : 0;

        //TODO Extend this with variable, such as op.pet.element
        if (id.TryTrimStart("defElementRelation", out trimId) &&
            trimId.TryTrimParentheses(out trimId) &&
            int.TryParse(trimId, out var elementId)) {
            return PetElementSystem.GetElementRelation(elementId, pet);
        }

        if (id.StartsWith("status") || id.StartsWith("powerup"))
            return GetPetStatusIdentifier(id, pet.statusController);

        if (id.TryTrimStart("buff", out trimId))
            return GetBuffIdentifier(trimId, pet.buffController);

        return pet.TryGetPetIdentifier(id, out num) ? num : GetNumIdentifier(id);
    }

    public static float GetPetStatusIdentifier(string id, PetBattleStatusController statusController) {
        string trimId = id;
        if (id.TryTrimStart("status.", out trimId)) {
            return trimId switch {
                "maxHp" => statusController.maxHp,
                "lostHp" => statusController.maxHp - statusController.battleStatus["hp"],
                "anger" => statusController.anger,
                "minAnger" => statusController.minAnger,
                "maxAnger" => statusController.maxAnger,
                _ => trimId.TryTrimStart("init", out var statusId) ? statusController.initStatus[statusId] : statusController.battleStatus[trimId],
            };
        }

        if (id.TryTrimStart("powerup.", out trimId)) {
            Status powerup = statusController.powerup;
            return trimId switch {
                "posCount" => powerup.Count(x => x > 0),
                "negCount" => powerup.Count(x => x < 0),
                "posSum" => powerup.Select(x => Mathf.Max(0, x)).sum,
                "negSum" => powerup.Select(x => Mathf.Min(0, x)).sum,
                "posMax" => powerup.posMax,
                "negMax" => powerup.negMax,
                _ => powerup[trimId],
            };
        }

        return GetNumIdentifier(id);
    }

    public static float GetSkillIdentifier(string id, UnitSkillSystem skillSystem) {
        float num = 0;
        
        if (skillSystem.TryGetSkillSystemIdentifier(id, out num))
            return num;
        
        return skillSystem.skill.TryGetSkillIdentifier(id, out num) ? num : GetNumIdentifier(id);
    }

    public static float GetBuffIdentifier(string id, PetBattleBuffController buffController) {
        List<Buff> buffs = buffController.buffs;

        if (id == ".count")
            return buffs.Count;

        if (id.StartsWith("[")) {
            int buffIdStartIdx = id.IndexOf('[');
            int buffIdEndIdx = id.IndexOf(']');
            string buffIdExpr = id.Substring(buffIdStartIdx + 1, buffIdEndIdx - buffIdStartIdx - 1);
            
            id = id.TrimStart("[" + buffIdExpr + "].");

            // Parse failure => buffIdExpr is BuffType.
            if (!int.TryParse(buffIdExpr, out int buffId)) {
                BuffType buffType = buffIdExpr.ToBuffType();

                return id switch {
                    "count" => buffs.Count(x => x.IsType(buffType)),
                    "block" => buffController.IsBuffTypeBlocked(buffType) ? 1 : 0,
                    _ => 0,
                };
            }

            // Parse success => buffIdExpr is buffId.
            Buff buff = buffs.Find(x => x.id == buffId) ?? new Buff(-1);

            return id switch {
                "count" => buffs.Count(x => x.id == buffId),
                "block" => buffController.IsBuffIdBlocked(buffId) ? 1 : 0,
                _ => buff.TryGetBuffIdentifier(id, out float num) ? num : GetNumIdentifier(id),
            };
        }
        return GetNumIdentifier(id);
    }
}
