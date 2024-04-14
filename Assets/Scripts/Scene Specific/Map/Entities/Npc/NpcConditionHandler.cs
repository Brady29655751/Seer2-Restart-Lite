using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;


public static class NpcConditionHandler
{
    public static Func<bool> GetNpcCondition(string op, string key, string value) {
        string trimKey = key;
        if (key.TryTrimStart("random", out trimKey))
            return () => GetRandom(op, trimKey, value);

        if (key.TryTrimStart("firstPet.", out trimKey))
            return () => GetFirstPet(op, trimKey, value);

        if (key.TryTrimStart("activity", out trimKey)) {
            string id = trimKey.TrimParentheses();
            string dataKey = trimKey.Substring(trimKey.IndexOf('.') + 1);
            var activity = Activity.Find(id);
            return () => GetActivity(activity, op, dataKey, value);
        }

        if (key.TryTrimStart("mission", out trimKey)) {
            int id = int.Parse(trimKey.TrimParentheses());
            string dataKey = trimKey.Substring(trimKey.IndexOf('.') + 1);
            var mission = Mission.Find(id);
            return () => GetMission(mission, op, dataKey, value);
        }

        if (key.TryTrimStart("battle.", out trimKey)) {
            return () => GetBattle(op, trimKey, value);
        }

        return () => true;
    }

    public static bool GetRandom(string op, string key, string value) {
        var range = key.TrimParentheses().ToIntList('~');
        int rng = Random.Range(range[0], range[1]);
        return Operator.Condition(op, rng, float.Parse(value));
    }

    public static bool GetFirstPet(string op, string key, string value) {
        return Operator.Condition(op, Player.instance.petBag[0].GetPetIdentifier(key), float.Parse(value));
    }

    public static bool GetMission(Mission mission, string op, string key, string value) {
        if (mission == null)
            return op != "=";


        bool result = key switch
        {
            "done" => mission.isDone == bool.Parse(value),
            _ => mission.checkPointId == value,
        };
        return (op == "=") ? result : (!result);
    }

    public static bool GetActivity(Activity activity, string op, string key, string value) {
        string type = "string";       
        if (value.StartsWith("[")) {
            type = value.TrimParentheses().ToLower();
            value = value.Substring(type.Length + 2);
        }
        string conditionVal = value.Substring(0, value.IndexOf("["));
        string defalutVal = value.Substring(value.IndexOf("]") + 1);
        string activityVal = activity.GetData(key, defalutVal);
        switch (type) {
            default:
                return (op == "=") ? (activityVal == conditionVal) : (activityVal != conditionVal);
            case "float":
            case "int":
            case "uint":
                return Operator.Condition(op, float.Parse(activityVal), float.Parse(conditionVal));
            case "bool":
                return bool.Parse(activityVal) == bool.Parse(conditionVal);
        }
    }

    public static bool GetBattle(string op, string key, string value) {
        var battle = Player.instance.currentBattle;
        if (battle == null)
            return false;

        return Operator.Condition(op, battle.GetBattleIdentifier(key), float.Parse(value));
    }
}
