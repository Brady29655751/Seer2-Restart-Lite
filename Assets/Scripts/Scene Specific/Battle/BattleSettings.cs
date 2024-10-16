using System;
using System.Xml.Serialization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleSettings
{
    [XmlAttribute("mod")] public bool isMod = false;
    [XmlAttribute("simulate")] public bool isSimulate = false;
    [XmlAttribute("escape")] public bool isEscapeOK = true;
    [XmlAttribute("capture")] public bool isCaptureOK = false;
    [XmlAttribute("item")] public bool isItemOK = true;
    [XmlElement("captureLevel")] public int captureLevel = 1;


    [XmlAttribute("count")] public int petCount = 6;
    [XmlAttribute("time")] public int time = 10;
    [XmlElement("mode")] public int modeId = 0;
    [XmlIgnore] public BattleMode mode {
        get => (BattleMode)modeId;
        set => modeId = (int)value;
    }
    [XmlElement("weather")] public int weather = 0;

    public BattleSettings() {}

    public BattleSettings(BattleMode _mode, int _petCount = 6, int _weather = 0, bool _isEscapeOK = true, bool _isSimulate = false, bool _isCapture = false, bool _isItem = true, bool _isMod = false) {
        isMod = _isMod;
        isSimulate = _isSimulate;
        isEscapeOK = _isEscapeOK;
        isCaptureOK = _isCapture;
        isItemOK = _isItem;

        mode = _mode;
        weather = _weather;
        petCount = _petCount;
    }

    public BattleSettings(BattleSettings rhs) {
        isMod = rhs.isMod;
        isSimulate = rhs.isSimulate;
        isEscapeOK = rhs.isEscapeOK;
        isCaptureOK = rhs.isCaptureOK;
        isItemOK = rhs.isItemOK;
        
        mode = rhs.mode;
        weather = rhs.weather;
        petCount = rhs.petCount;
        time = rhs.time;
    }

    public float GetSettingsIdentifier(string id) {
        return id switch {
            "mode" => modeId,
            "petCount" => petCount,
            "time" => time,
            "weather" => weather,
            "mod" => isMod ? 1 : 0,
            "simulate" => isSimulate ? 1 : 0,
            "escape" => isEscapeOK ? 1 : 0,
            "item" => isItemOK ? 1 : 0,
            "capture" => isCaptureOK ? 1 : 0,
            "captureLevel" => isCaptureOK ? captureLevel : -1,
            _ => float.MinValue,
        };
    }

}

public enum BattleMode {
    Normal = 0,
    SelfSimulation = 1,
    PVP = 2,
    SPT = 3,
    Special = 4,
    Surprise = 5,
}
