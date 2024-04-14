using System;
using System.Xml.Serialization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[XmlRoot("mission")]
public class MissionInfo
{
    [XmlAttribute] public int id;
    [XmlAttribute("type")] public int typeId;
    [XmlIgnore] public MissionType type => (MissionType)typeId;

    [XmlElement("preMission")] public string preMissionId;    // 前置任務
    [XmlElement("nextMission")] public string nextMissionId;  // 解鎖任務

    [XmlIgnore] public List<int> preMissions => preMissionId.ToIntList();
    [XmlIgnore] public List<int> nextMissions => nextMissionId.ToIntList();


    [XmlElement("title")] public string title;
    
    [XmlArray("reward"), XmlArrayItem(typeof(Item), ElementName = "item")] 
    public List<Item> rewards;

    [XmlArray("checkpoint"), XmlArrayItem(typeof(MissionCheckpoint), ElementName = "branch")] 
    public List<MissionCheckpoint> checkpoints;
}

public class MissionCheckpoint {
    [XmlAttribute] public string id;
    [XmlElement("map")] public int mapId;
    [XmlElement("description")] public string intro;
    [XmlIgnore] public string description => intro.GetDescription();
}

public enum MissionType {
    None = -1,
    Main = 0,
    Side = 1,
    Daily = 2,
    Event = 3,
}
