using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using ExitGames.Client.Photon.StructWrapping;

public class DoorModel : Module
{
    public string door { get; private set; }
    public string mode { get; private set;}
    public string modeWithParentheses => string.IsNullOrEmpty(mode) ? string.Empty : $"[{mode}]";
    public string floor => battleInfo.id;
    public int petNum => GetPetNum();

    public int doorIndex => DoorPanel.doorNames.IndexOf(door);
    public string floorKey => door + modeWithParentheses + "[floor]";
    public string floorData => activity.GetData(floorKey, "1");
    public int floorNum => int.Parse(floorData);

    public string maxFloorKey => door + modeWithParentheses + "[max]";
    public string maxFloorData => activity.GetData(maxFloorKey, "0");
    public int floorMax => int.Parse(maxFloorData);

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
            "competition" => 3,
            "twin" => 2,
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
        switch (door) 
        {
            default:    
                NpcActionHandler.StartBattle(npcInfo, new NpcButtonHandler() {
                    param = new List<string>() { floor },
                });
                return;

            case "competition":
                if ((floorNum > 21) || (floorNum % 7 == 0))
                    goto default;

                void WinHandler()
                {
                    if (floorMax < floorNum)
                        activity.SetData(maxFloorKey, floorNum.ToString());

                    activity.SetData(floorKey, (floorNum + 1).ToString());

                    Item reward = new Item(3, ((mode == "easy") ? 1 : 2) * 10);
                    Item.Add(reward);
                    Item.OpenHintbox(reward);
                };
                BattleSettings settings = new BattleSettings()
                {
                    petCount = 3,
                    mode = BattleMode.SelfSimulation,
                    initBuffExpr = "(rule:" + Buff.BUFFID_PET_RANDOM + ")",
                    isItemOK = mode == "easy",
                    isSimulate = true,
                };
                BattleInfo battleInfo = new BattleInfo() 
                {
                    settings = settings,
                    playerInfo = Enumerable.Range(0, 3).Select(x => BossInfo.GetRandomEnemyInfo()).ToList(),
                    enemyInfo = Enumerable.Range(0, 3).Select(x => BossInfo.GetRandomEnemyInfo()).ToList(),
                    winHandler = NpcButtonHandler.Callback(WinHandler).SingleToList(),
                    loseHandler = new List<NpcButtonHandler>(),
                };
                Battle battle = new Battle(battleInfo);
                SceneLoader.instance.ChangeScene(SceneId.Battle);
                return;
        }
    }

    public void RestartFromFirstFloor() {
        activity.SetData(floorKey, "1");
        SaveSystem.SaveData();
    }
}
