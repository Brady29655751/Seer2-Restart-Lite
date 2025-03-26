using System;
using System.Xml.Serialization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleRecord
{
    [XmlIgnore] public const int MAX_RECORD_STORAGE_NUM = 15;
    public bool isMaster;
    public BattleResultState originalResultState = BattleResultState.Error;
    public BattleSettings settings;
    public Pet[] masterPetBag, clientPetBag;
    public DateTime date;

    [XmlArray("actionList"), XmlArrayItem(typeof(IKeyValuePair<string[], bool>), ElementName = "action")] 
    public List<IKeyValuePair<string[], bool>> actionList = new List<IKeyValuePair<string[], bool>>();


    [XmlIgnore] public Pet[] myPetBag => isMaster ? masterPetBag : clientPetBag;
    [XmlIgnore] public Pet[] opPetBag => isMaster ? clientPetBag : masterPetBag;
    [XmlIgnore] public BattleResultState resultState => GetRecordResultState();
    [XmlIgnore] public Color resultColor => GetResultColor(resultState);
    [XmlIgnore] public string resultNote => GetResultNote(resultState);

    public static void VersionUpdate() {
        Player.instance.gameData.battleRecordStorage = new List<BattleRecord>();
    }

    public BattleRecord(){}

    public BattleResultState GetRecordResultState() {
        return originalResultState switch {
            BattleResultState.Win       =>  BattleResultState.Win,
            BattleResultState.Lose      =>  BattleResultState.Lose,
            BattleResultState.MyEscape  =>  BattleResultState.Lose,
            BattleResultState.OpEscape  =>  BattleResultState.Win,
            _   =>  BattleResultState.Error,
        };
    }

    public static Color GetResultColor(BattleResultState resultState) {
        return resultState switch {
            BattleResultState.Win   =>  ColorHelper.gold,
            BattleResultState.Lose  =>  Color.cyan,
            _  =>  Color.white,
        };
    }

    public static string GetResultNote(BattleResultState resultState) {
        return resultState switch {
            BattleResultState.Win   =>  "胜利",
            BattleResultState.Lose  =>  "落败",
            _  =>  "中断",
        };
    }

    public void AddAction(string[] action, bool isMe) {
        actionList.Add(new IKeyValuePair<string[], bool>(action, isMe));
    }

}
