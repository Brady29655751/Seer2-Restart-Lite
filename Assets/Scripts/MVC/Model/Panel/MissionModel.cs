using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissionModel : Module
{
    private List<Mission> storage;
    public List<Mission> selections { get; private set; }
    public MissionType type { get; private set; }
    public int id { get; private set; }
    public Mission currentMission => id.IsInRange(0, selections.Count) ? selections[id] : null;

    public void SetStorage(List<Mission> storage, MissionType type = MissionType.Main) {
        this.storage = storage;
        SetFilterType(type);
    }

    public void SetFilterType(MissionType type) {
        this.type = type;
        selections = storage.FindAll(x => x.info.type == type);
    }

    public void Select(int index) {
        id = index;
    }

}
