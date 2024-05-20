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

            int startIndex = trimId.IndexOf('[');
            int middleIndex = trimId.IndexOf('~');
            int endIndex = trimId.IndexOf(']');

            if (middleIndex == -1)
                return trimId.Substring(startIndex + 1, endIndex - startIndex - 1).ToIntList('|').Random();

            string startExpr = trimId.Substring(startIndex + 1, middleIndex - startIndex - 1);
            string endExpr = trimId.Substring(middleIndex + 1, endIndex - middleIndex - 1);
            int startRange = int.Parse(startExpr);
            int endRange = int.Parse(endExpr);

            return Random.Range(startRange, endRange + 1);
        }
        return float.TryParse(id, out num) ? num : 0;
    }

    public static float GetIdentifier(string id, Effect effect, Unit lhsUnit, Unit rhsUnit) {
        float num = 0;
        return effect.source switch {
            Buff buff => buff.TryGetBuffIdentifier(id, out num) ? num : GetGlobalIdentifier(id, lhsUnit, rhsUnit),
            Skill skill => skill.TryGetSkillIdentifier(id, out num) ? num : GetGlobalIdentifier(id, lhsUnit, rhsUnit),
            _ => GetGlobalIdentifier(id, lhsUnit, rhsUnit),
        };
    }

    public static float GetGlobalIdentifier(string id, Unit lhsUnit, Unit rhsUnit) {
        string trimId = id;
            
        if (id.TryTrimStart("state.", out trimId))
            return Player.instance.currentBattle.GetBattleIdentifier(trimId);
            
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
            return trimId switch {
                "cursor" => unit.petSystem.cursor,
                _ => GetPetIdentifier(trimId, unit.pet),
            };

        if (id.TryTrimStart("skill.", out trimId))
            return GetSkillIdentifier(trimId, unit.skillSystem);

        return GetNumIdentifier(id);
    }

    public static float GetPetIdentifier(string id, BattlePet pet) {
        string trimId = id;

        if (id == "movable")
            return pet.isMovable ? 1 : 0;

        if (id == "dead")
            return pet.isDead ? 1 : 0;

        if (id.StartsWith("status") || id.StartsWith("powerup"))
            return GetPetStatusIdentifier(id, pet.statusController);

        if (id.TryTrimStart("buff", out trimId))
            return GetBuffIdentifier(trimId, pet.buffController);

        float num = 0;
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
                _ => statusController.battleStatus[trimId],
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
                    "count" => buffs.Count(x => x.info.type == buffType),
                    "block" => buffController.IsBuffTypeBlocked(buffType) ? 1 : 0,
                    _ => 0,
                };
            }

            // Parse success => buffIdExpr is buffId.
            Buff buff = buffs.Find(x => x.id == buffId) ?? new Buff(-1);

            return id switch {
                "count" => buffs.Count(x => x.id == buffId),
                "block" => buffController.IsBuffBlocked(buffId) ? 1 : 0,
                _ => buff.TryGetBuffIdentifier(id, out float num) ? num : GetNumIdentifier(id),
            };
        }
        return GetNumIdentifier(id);
    }
}
