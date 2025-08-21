using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;


public static class NpcConditionHandler
{
    public static Func<bool> GetNpcCondition(string op, string key, string value) {
        string trimKey = key;
        if (key.TryTrimStart("random", out trimKey))
            return () => GetRandom(op, trimKey, value);

        if (key.TryTrimStart("map.", out trimKey))
            return () => Operator.Condition(op, Player.instance.currentMap?.GetMapIdentifier(trimKey) ?? 0, float.Parse(value));

        if (key.TryTrimStart("npc", out trimKey))
        {
            int id = int.Parse(trimKey.TrimParentheses());
            string dataKey = trimKey.Substring(trimKey.IndexOf('.') + 1);
            var npcDict = (Dictionary<int, NpcController>)Player.GetSceneData("mapNpcList");
            var npc = npcDict.Get(id);
            return () => dataKey switch
            {
                "color" => (op == "=") ^ (((Color)npc.GetNpcIdentifier("color")) != value.ToColor()),
                _ => false,
            };
        }

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

        if (key.TryTrimStart("item", out trimKey)) {
            int id = int.Parse(trimKey.TrimParentheses());
            string dataKey = trimKey.Substring(trimKey.IndexOf('.') + 1);
            Item item = Item.Find(id, trimKey.StartsWith("_yite") ? YiTeRogueData.instance.itemBag : null);
            return () => GetItem(item, op, dataKey, value);
        }

        if (key.TryTrimStart("yite_rogue.", out trimKey)) {
            return () => GetYiTeRogue(op, trimKey, value);
        }

        return () => Operator.Condition(op, Parser.ParseOperation(key), Parser.ParseOperation(value));
    }

    public static bool GetRandom(string op, string key, string value) {
        bool result = Operator.Condition(op, Player.instance.random, float.Parse(value));

        if (key == "[old]")
            return result;
            

        if (key == "[new]") {
            Player.instance.random = Random.Range(0, 100);
            return result;
        }

        var range = key.TrimParentheses().ToIntList('~');
        int rng = Random.Range(range[0], range[1]);
        
        if (Player.instance.petBag.Any(x => (x != null) && ((Buff.GetEmblemBuff(x)?.id ?? 0) == 20_0051)))
            rng /= 4;

        return Operator.Condition(op, rng, float.Parse(value));
    }

    public static bool GetFirstPet(string op, string key, string value) {
        return Operator.Condition(op, Player.instance.petBag[0].GetPetIdentifier(key), float.Parse(value));
    }

    public static bool GetMission(Mission mission, string op, string key, string value) {
        if (mission == null)
            return op != "=";

        bool result = key switch {
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
        string defaultVal = value.Substring(value.IndexOf("]") + 1);
        string activityVal = activity.GetData(key, defaultVal);
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
            return op != "=";

        return Operator.Condition(op, Identifier.GetBattleIdentifier("state." + key, battle.currentState.myUnit, battle.currentState.opUnit), float.Parse(value));
    }

    public static bool GetItem(Item item, string op, string key, string value) {
        return key switch {
            _ => Operator.Condition(op, item?.num ?? 0, int.Parse(value)),
        };
    }

    public static bool GetYiTeRogue(string op, string key, string value) {
        var rogue = YiTeRogueData.instance;
        if (rogue == null)
            return op != "=";

        return Operator.Condition(op, YiTeRogueData.instance.GetYiTeRogueIdentifier(key), float.Parse(value));
    }
}
