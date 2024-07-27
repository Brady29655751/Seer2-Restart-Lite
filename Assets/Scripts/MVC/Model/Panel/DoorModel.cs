using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorModel : Module
{
    public string door { get; private set; }
    public string mode { get; private set;}
    public string floor => battleInfo.id;
    public int petNum => GetPetNum();

    public int doorIndex => DoorPanel.doorNames.IndexOf(door);
    public string floorKey => door + "[" + mode + "]" + "[floor]";
    public string floorData => activity.GetData(floorKey, "1");

    public NpcInfo npcInfo => DialogManager.instance.currentNpc.GetInfo();
    public BattleInfo battleInfo => npcInfo.battleHandler.Find(x => x.id == floorData) ?? npcInfo.battleHandler.Aggregate((x, y) => (int.Parse(x.id) > int.Parse(y.id)) ? x : y);
    public Activity activity = null;

    protected override void Awake() {
        activity = Activity.Find("door");
    }

    public int GetPetNum() {
        return door switch {
            "challenge" => 1,
            "brave" => 3,
            _ => 6
        };
    }

    public void SetDoor(string doorName) {
        door = doorName;
    }

    public void SetMode(string modeName) {
        mode = modeName;
    }

    public void StartBattle() {
        NpcActionHandler.StartBattle(npcInfo, new NpcButtonHandler() {
            param = new List<string>() { floor },
        });
    }

    public void RestartFromFirstFloor() {
        activity.SetData(floorKey, "1");
        SaveSystem.SaveData();
    }
}
