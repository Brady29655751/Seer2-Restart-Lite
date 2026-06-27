using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VolumeSettingModel : Module
{
    public const string BubbleStyleWhite = "white";
    public const string BubbleStyleBlueBlack = "blue-black";

    public SettingsData settingsData => Player.instance.gameData.settingsData;
    public float BGMVolume { get; private set; }
    public float UIVolume { get; private set; }
    public float battleBGMVolume { get; private set; }
    public float battleSEVolume { get; private set; }
    public bool flashWhenBigDamage { get; private set; }
    public bool shakeWhenBigDamage { get; private set; }

    public float battleAnimSpeed { get; private set; }
    public bool autoHealAfterBattle { get; private set; }
    public int initMapId { get; private set; }
    public string wildNpcBubbleStyle { get; private set; }
    public ComboDamageDisplayMode comboDamageDisplayMode { get; private set; }

    public void InitVolume()
    {
        BGMVolume = settingsData.BGMVolume;
        UIVolume = settingsData.UIVolume;
        battleBGMVolume = settingsData.battleBGMVolume;
        battleSEVolume = settingsData.battleSEVolume;
        flashWhenBigDamage = settingsData.flashWhenBigDamage;
        shakeWhenBigDamage = settingsData.shakeWhenBigDamage;
        battleAnimSpeed = settingsData.battleAnimSpeed;
        autoHealAfterBattle = settingsData.autoHealAfterBattle;
        initMapId = settingsData.initMapId;
        wildNpcBubbleStyle = NormalizeWildNpcBubbleStyle(settingsData.wildNpcBubbleStyle);
        comboDamageDisplayMode = NormalizeComboDamageDisplayMode(settingsData.comboDamageDisplayMode);
    }

    public void OnConfirmSettings()
    {
        settingsData.BGMVolume = BGMVolume;
        settingsData.UIVolume = UIVolume;
        settingsData.battleBGMVolume = battleBGMVolume;
        settingsData.battleSEVolume = battleSEVolume;
        settingsData.flashWhenBigDamage = flashWhenBigDamage;
        settingsData.shakeWhenBigDamage = shakeWhenBigDamage;
        settingsData.battleAnimSpeed = battleAnimSpeed;
        settingsData.autoHealAfterBattle = autoHealAfterBattle;
        settingsData.initMapId = initMapId;
        settingsData.wildNpcBubbleStyle = NormalizeWildNpcBubbleStyle(wildNpcBubbleStyle);
        settingsData.comboDamageDisplayMode = (int)NormalizeComboDamageDisplayMode(comboDamageDisplayMode);
        SaveSystem.SaveData();
    }

    public void SetBGMVolume(float value)
    {
        BGMVolume = value;
    }
    public void SetUIVolume(float value)
    {
        UIVolume = value;
    }
    public void SetBattleBGMVolume(float value)
    {
        battleBGMVolume = value;
    }
    public void SetBattleSEVolume(float value)
    {
        battleSEVolume = value;
    }

    public void SetFlash(bool isOn)
    {
        flashWhenBigDamage = isOn;
    }

    public void SetShake(bool isOn)
    {
        shakeWhenBigDamage = isOn;
    }

    public void SetBattleAnimSpeed(float value)
    {
        battleAnimSpeed = value;
    }

    public void SetAutoHealAfterBattle(bool isOn)
    {
        autoHealAfterBattle = isOn;
    }

    public void SetInitMapId(int value)
    {
        initMapId = value;
    }

    public void SetWildNpcBubbleStyle(string value)
    {
        wildNpcBubbleStyle = NormalizeWildNpcBubbleStyle(value);
    }

    public void SetComboDamageDisplayMode(ComboDamageDisplayMode value)
    {
        comboDamageDisplayMode = NormalizeComboDamageDisplayMode(value);
    }

    public void ToggleWildNpcBubbleStyle()
    {
        SetWildNpcBubbleStyle(wildNpcBubbleStyle == BubbleStyleBlueBlack ? BubbleStyleWhite : BubbleStyleBlueBlack);
    }

    public void ToggleComboDamageDisplayMode()
    {
        SetComboDamageDisplayMode(comboDamageDisplayMode switch
        {
            ComboDamageDisplayMode.TotalOnly => ComboDamageDisplayMode.AllComboOnly,
            ComboDamageDisplayMode.AllComboOnly => ComboDamageDisplayMode.AllComboAndTotal,
            _ => ComboDamageDisplayMode.TotalOnly,
        });
    }

    public static string NormalizeWildNpcBubbleStyle(string value)
    {
        return string.Equals(value, BubbleStyleBlueBlack, StringComparison.OrdinalIgnoreCase)
            ? BubbleStyleBlueBlack
            : BubbleStyleWhite;
    }

    public static string GetWildNpcBubbleStyleLabel(string value)
    {
        return NormalizeWildNpcBubbleStyle(value) == BubbleStyleBlueBlack ? "蓝黑" : "纯白";
    }
    public static ComboDamageDisplayMode NormalizeComboDamageDisplayMode(int value)
    {
        return NormalizeComboDamageDisplayMode((ComboDamageDisplayMode)value);
    }

    public static ComboDamageDisplayMode NormalizeComboDamageDisplayMode(ComboDamageDisplayMode value)
    {
        return value switch
        {
            ComboDamageDisplayMode.TotalOnly => ComboDamageDisplayMode.TotalOnly,
            ComboDamageDisplayMode.AllComboAndTotal => ComboDamageDisplayMode.AllComboAndTotal,
            _ => ComboDamageDisplayMode.AllComboOnly,
        };
    }

    public static string GetComboDamageDisplayModeLabel(ComboDamageDisplayMode value)
    {
        return NormalizeComboDamageDisplayMode(value) switch
        {
            ComboDamageDisplayMode.TotalOnly => "仅显示总伤害",
            ComboDamageDisplayMode.AllComboOnly => "仅显示所有连击伤害",
            _ => "显示所有连击伤害和总伤害",
        };
    }
}
