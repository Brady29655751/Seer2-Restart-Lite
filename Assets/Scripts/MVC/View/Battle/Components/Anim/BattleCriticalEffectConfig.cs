using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public class BattleCriticalEffectConfig
{
    private const string ConfigPath = "Data/Battle/critical_effect.csv";
    private const int ColumnCount = 12;
    private const string CriticalEventType = "critical";
    private const string PosterEffectType = "poster";
    private static Dictionary<string, BattleCriticalEffectConfig> configDict;

    public readonly string OwnerType;
    public readonly int OwnerId;
    public readonly string EventType;
    public readonly string EffectType;
    public readonly string AssetKey;
    public readonly bool IsMod;
    public readonly Vector2 AnchoredPosition;
    public readonly float Scale;
    public readonly float Duration;
    public readonly Vector2 MoveOffset;

    private BattleCriticalEffectConfig(string[] data, int index)
    {
        OwnerType = NormalizeOwnerType(data[index]);
        OwnerId = ParseInt(data[index + 1]);
        EventType = NormalizeKey(data[index + 2]);
        EffectType = NormalizeKey(data[index + 3]);
        AssetKey = data[index + 4];
        IsMod = ParseBool(data[index + 5]);
        AnchoredPosition = new Vector2(ParseFloat(data[index + 6]), ParseFloat(data[index + 7]));
        Scale = ParseFloat(data[index + 8], 1f);
        Duration = ParseFloat(data[index + 9], 0.75f);
        MoveOffset = new Vector2(ParseFloat(data[index + 10]), ParseFloat(data[index + 11]));
    }

    public static BattleCriticalEffectConfig GetCriticalPoster(int petAnimId, int petBaseId)
    {
        Init();
        return Get("anim", petAnimId, CriticalEventType, PosterEffectType) ??
               Get("base", petBaseId, CriticalEventType, PosterEffectType);
    }

    private static BattleCriticalEffectConfig Get(string ownerType, int ownerId, string eventType, string effectType)
    {
        if (ownerId == 0)
            return null;

        return configDict.Get(GetConfigKey(ownerType, ownerId, eventType, effectType));
    }

    private static void Init()
    {
        if (configDict != null)
            return;

        configDict = new Dictionary<string, BattleCriticalEffectConfig>();
        string[] data = ResourceManager.LoadCSV(ConfigPath);
        if (data == null)
            return;

        for (int i = 0; i + ColumnCount <= data.Length; i += ColumnCount)
        {
            if (!int.TryParse(data[i + 1], out _))
                continue;

            var config = new BattleCriticalEffectConfig(data, i);
            if (string.IsNullOrEmpty(config.OwnerType) || string.IsNullOrEmpty(config.EventType) ||
                string.IsNullOrEmpty(config.EffectType) || string.IsNullOrEmpty(config.AssetKey))
            {
                continue;
            }

            configDict.Set(GetConfigKey(config.OwnerType, config.OwnerId, config.EventType, config.EffectType), config);
        }
    }

    private static string GetConfigKey(string ownerType, int ownerId, string eventType, string effectType)
    {
        return $"{NormalizeOwnerType(ownerType)}:{ownerId}:{NormalizeKey(eventType)}:{NormalizeKey(effectType)}";
    }

    private static string NormalizeOwnerType(string value)
    {
        value = NormalizeKey(value);
        switch (value)
        {
            case "animation":
            case "animid":
                return "anim";
            case "baseid":
            case "petbase":
            case "petbaseid":
                return "base";
            default:
                return value;
        }
    }

    private static string NormalizeKey(string value)
    {
        return string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToLowerInvariant();
    }

    private static int ParseInt(string value, int defaultValue = 0)
    {
        return int.TryParse(value, out var result) ? result : defaultValue;
    }

    private static float ParseFloat(string value, float defaultValue = 0)
    {
        return float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result)
            ? result
            : defaultValue;
    }

    private static bool ParseBool(string value)
    {
        return bool.TryParse(value, out var result) && result;
    }
}
