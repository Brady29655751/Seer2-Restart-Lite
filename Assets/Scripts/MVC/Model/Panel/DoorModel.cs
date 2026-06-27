using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using ExitGames.Client.Photon.StructWrapping;

public class DoorModel : Module
{
    public string door { get; private set; }
    public string mode { get; private set; }
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
    public BattleInfo battleInfo => GetBattleInfo();
    public Activity activity = null;

    protected override void Awake()
    {
        activity = Activity.Find("door");
    }

    public int GetPetNum()
    {
        return door switch
        {
            "challenge" => 1,
            "brave" => 3,
            "competition" => 3,
            "hero" => 6,
            "twin" => 2,
            "new_basic" => 1,
            _ => 6
        };
    }

    // battleInfo for showing enemy info
    public BattleInfo GetBattleInfo()
    {
        switch (door)
        {
            default:
                return npcInfo.battleHandler.Find(x => x.id == floorData) ?? npcInfo.battleHandler.Aggregate((x, y) => (int.Parse(x.id) > int.Parse(y.id)) ? x : y);

            case "competition":
            case "hero":
                if ((floorNum > 21) || (floorNum % 7 == 0))
                    goto default;
                
                return new BattleInfo(){ enemyInfo = new List<BossInfo>() };

            case "new_basic":
                if (floorNum > 29)
                    goto default;

                return new BattleInfo()
                {
                    enemyInfo = new BossInfo()
                    { 
                        petId = 10000 + floorNum,
                        level = 10 + (floorNum - 1) * 3,
                        initBuffIds = "-3",
                    }.SingleToList(),
                };
        }
    }

    public void SetDoor(string doorName)
    {
        door = doorName;
    }

    public void SetMode(string modeName)
    {
        mode = modeName;
    }

    public void StartBattle()
    {
        void WinHandler()
        {
            if (floorMax < floorNum)
                activity.SetData(maxFloorKey, floorNum.ToString());
            
            activity.SetData(floorKey, (floorNum + 1).ToString());

            Item reward = new Item(3, ((mode == "easy") ? 1 : 2) * 10);
            Item.Add(reward);
            Item.OpenHintbox(reward);
        };

        BattleSettings doorSettings;
        BattleInfo doorBattleInfo;
        Battle doorBattle;

        switch (door)
        {
            default:
                NpcActionHandler.StartBattle(npcInfo, new NpcButtonHandler()
                {
                    param = new List<string>() { floor },
                });
                return;

            case "competition":
            case "hero":
                if ((floorNum > 21) || (floorNum % 7 == 0))
                    goto default;
                
                doorSettings = new BattleSettings()
                {
                    petCount = door == "competition" ? 3 : 6,
                    mode = BattleMode.SelfSimulation,
                    initBuffExpr = "(rule:" + Buff.BUFFID_PET_RANDOM + ")",
                    isItemOK = mode == "easy",
                    isSimulate = true,
                };
                doorBattleInfo = new BattleInfo()
                {
                    settings = doorSettings,
                    playerInfo = BossInfo.GetRandomEnemyInfoList(count: doorSettings.petCount, withMod: door == "hero"),
                    enemyInfo = BossInfo.GetRandomEnemyInfoList(count: doorSettings.petCount, withMod: door == "hero"),
                    winHandler = NpcButtonHandler.Callback(WinHandler).SingleToList(),
                    loseHandler = new List<NpcButtonHandler>(),
                };
                doorBattle = new Battle(doorBattleInfo);
                SceneLoader.instance.ChangeScene(SceneId.Battle);
                return;

            case "new_basic":
                if (floorNum > 29)
                    goto default;
                
                doorSettings = new BattleSettings()
                {
                    petCount = 1,
                    mode = BattleMode.SelfSimulation,
                    isSimulate = true,
                };
                doorBattleInfo = new BattleInfo()
                {
                    settings = doorSettings,
                    enemyInfo = new BossInfo()
                    { 
                        petId = 10000 + floorNum,
                        level = 10 + (floorNum - 1) * 3,
                        initBuffIds = "-3",
                    }.SingleToList(),
                    winHandler = NpcButtonHandler.Callback(WinHandler).SingleToList(),
                    loseHandler = new List<NpcButtonHandler>(),
                };
                doorBattle = new Battle(doorBattleInfo);
                SceneLoader.instance.ChangeScene(SceneId.Battle);
                return;
        }
    }

    public void RestartFromFirstFloor()
    {
        activity.SetData(floorKey, "1");
        SaveSystem.SaveData();
    }
}
