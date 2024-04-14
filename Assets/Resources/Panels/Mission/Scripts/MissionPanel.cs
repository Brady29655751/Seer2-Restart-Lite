using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissionPanel : Panel
{
    [SerializeField] private MissionController missionController;

    public override void Init()
    {
        base.Init();
        SetMissionStorage();
    }

    public void SetMissionStorage() {
        var storage = Player.instance.gameData.missionStorage.FindAll(x => !x.isDone);
        missionController.SetMissionStorage(storage);
    }
}
