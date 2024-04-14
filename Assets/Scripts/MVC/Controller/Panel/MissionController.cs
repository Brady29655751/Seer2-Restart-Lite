using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissionController : Module
{
    [SerializeField] private MissionModel missionModel;
    [SerializeField] private MissionListView missionListView;
    [SerializeField] private MissionContentView missionContentView;

    public void SetMissionStorage(List<Mission> missions) {
        missionModel.SetStorage(missions, MissionType.Main);
        OnSetMissionList();
    }

    public void OnSetMissionList() {
        missionListView.SetStorage(missionModel.selections, Select);
        Select(0);
    }

    public void SetType(int index) {
        MissionType type = (MissionType)index;
        if (type == missionModel.type)
            return;
        
        missionModel.SetFilterType(type);
        OnSetMissionList();
    } 

    public void Select(int index) {
        missionModel.Select(index);
        missionContentView?.SetMission(missionModel.currentMission);
    }

    public void MissionStart() {
        if (missionModel.currentMission == null)
            return;
            
        TeleportHandler.Teleport(missionModel.currentMission.checkpointInfo.mapId);
    }

}
