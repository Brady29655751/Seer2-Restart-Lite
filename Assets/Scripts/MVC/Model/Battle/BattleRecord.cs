using System;
using System.Xml.Serialization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleRecord
{
    public bool isMaster;
    public BattleResultState resultState = BattleResultState.Error;
    public BattleSettings settings;
    public Pet[] masterPetBag, clientPetBag;
    public DateTime date;

    [XmlArray("actionList"), XmlArrayItem(typeof(IKeyValuePair<string[], bool>), ElementName = "action")] 
    public List<IKeyValuePair<string[], bool>> actionList = new List<IKeyValuePair<string[], bool>>();


    public static void VersionUpdate() {
        Player.instance.gameData.battleRecordStorage = new List<BattleRecord>();
    }

    public BattleRecord(){}

    public BattleResultState GetRecordResultState() {
        if (isMaster)
            return resultState;

        return resultState switch {
            BattleResultState.Win   =>  BattleResultState.Lose,
            BattleResultState.Lose  =>  BattleResultState.Win,
            _   =>  resultState,
        };
    }

    public void AddAction(string[] action, bool isMe) {
        actionList.Add(new IKeyValuePair<string[], bool>(action, isMe));
    }

}
