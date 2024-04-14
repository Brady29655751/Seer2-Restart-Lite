using System.Linq;
using System.Xml.Serialization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Mission
{
    public MissionInfo info => GetMissionInfo(id);
    public MissionCheckpoint checkpointInfo => info.checkpoints.Find(x => x.id == checkPointId);

    [XmlAttribute] public int id;
    [XmlAttribute("checkpoint")] public string checkPointId;
    [XmlAttribute("done")] public bool isDone = false;

    public Mission() {}

    public Mission(int id) {
        this.id = id;
        checkPointId = "default";
        isDone = false;
    }

    public static MissionInfo GetMissionInfo(int id) {
        return Database.instance.GetMissionInfo(id);
    }

    public static List<Mission> Filter(Predicate<Mission> pred) {
        return Player.instance.gameData.missionStorage.FindAll(pred);
    }

    public static Mission Find(int id = 0) {
        id = (id == 0) ? Player.instance.currentMissionId : id;
        Mission mission = Player.instance.gameData.missionStorage.Find(x => x.id == id);
        return mission;
    }

    public static Mission Start(int id) {
        Mission mission = new Mission(id);
        if (mission.info.preMissions != null) {
            foreach (var preId in mission.info.preMissions) {
                Mission preMission = Mission.Find(preId);
                if ((preMission == null) || (!preMission.isDone)) {
                    return null;
                }
            }
        }
        Player.instance.gameData.missionStorage.Add(mission);
        return mission;
    }

    public static void Checkpoint(int id, string checkpointId) {
        Mission mission = Mission.Find(id);
        if (mission == null)
            return;

        mission.checkPointId = checkpointId;
    }

    public static void Complete(int id = 0) {
        Mission mission = Mission.Find(id);

        mission.isDone = true;
        mission.checkPointId = "complete";
        if (mission.info.nextMissionId == null)
            return;

        foreach (var nextId in mission.info.nextMissionId) {
            Mission.Start(nextId);
        }
    }

    public static void VersionUpdate() {
        var mainMission = Mission.Filter(x => x.id <= 10000);
        if (mainMission.Count == 0)
            Mission.Start(1);
        else {
            Mission maxMainMission = mainMission.Aggregate((x, y) => x.id > y.id ? x : y);
            if (maxMainMission.isDone && GameManager.versionData.missionData.mainMissionCount > maxMainMission.id) {
                Mission.Start(maxMainMission.id + 1);
            }   
        }

        var sideMissions = Database.instance.missionInfos.FindAll(x => x.type == MissionType.Side);
        for (int i = 0; i < sideMissions.Count; i++) {
            Mission.Start(sideMissions[i].id);
        }
    }

    public static void DailyLogin() {
        var missionStorage = Player.instance.gameData.missionStorage;
        for (int i = 0; i < missionStorage.Count; i++) {
            if (missionStorage[i].info.type != MissionType.Daily)
                continue;

            missionStorage[i] = new Mission(missionStorage[i].id);
        }
    }

}
