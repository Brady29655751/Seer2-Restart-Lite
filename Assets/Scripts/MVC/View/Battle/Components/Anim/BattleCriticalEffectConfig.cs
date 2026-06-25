using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public class BattleCriticalEffectConfig
{
    private const string ConfigPath = "Data/Battle/critical_effect.csv";
    private const int ColumnCount = 8;
    private static Dictionary<int, BattleCriticalEffectConfig> configDict;

    public readonly int PetBaseId;
    public readonly string ResourcePath;
    public readonly Vector2 AnchoredPosition;
    public readonly float Scale;
    public readonly float Duration;
    public readonly Vector2 MoveOffset;

    private BattleCriticalEffectConfig(string[] data, int index)
    {
        PetBaseId = ParseInt(data[index]);
        ResourcePath = data[index + 1];
        AnchoredPosition = new Vector2(ParseFloat(data[index + 2]), ParseFloat(data[index + 3]));
        Scale = ParseFloat(data[index + 4], 1f);
        Duration = ParseFloat(data[index + 5], 0.75f);
        MoveOffset = new Vector2(ParseFloat(data[index + 6]), ParseFloat(data[index + 7]));
    }

    public static BattleCriticalEffectConfig Get(int petBaseId)
    {
        Init();
        return configDict.Get(petBaseId);
    }

    private static void Init()
    {
        if (configDict != null)
            return;

        configDict = new Dictionary<int, BattleCriticalEffectConfig>();
        string[] data = ResourceManager.LoadCSV(ConfigPath);
        if (data == null)
            return;

        for (int i = 0; i + ColumnCount <= data.Length; i += ColumnCount)
        {
            if (!int.TryParse(data[i], out _))
                continue;

            var config = new BattleCriticalEffectConfig(data, i);
            configDict.Set(config.PetBaseId, config);
        }
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
}
