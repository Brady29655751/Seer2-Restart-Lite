using System;
using System.Linq;
using System.Xml.Serialization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[XmlRoot("gameData")]
public class GameData
{
    public string version;
    public bool gender;
    public string nickname;
    [XmlIgnore] public int coin {
        get => Item.Find(Item.COIN_ID)?.num ?? 0;
        set {
            var item = Item.Find(Item.COIN_ID);
            if (item == null)
                Item.Add(new Item(Item.COIN_ID, value));
            else 
                item.num = value;
        }
    }
    [XmlIgnore] public int diamond {
        get => Item.Find(Item.DIAMOND_ID)?.num ?? 0;
        set {
            var item = Item.Find(Item.DIAMOND_ID);
            if (item == null)
                Item.Add(new Item(Item.DIAMOND_ID, value));
            else 
                item.num = value;
        }
    }
    public DateTime firstLoginDate;
    public DateTime lastLoginDate;
    public int achievement;

    public Pet[] petBag = new Pet[6];
    public List<Pet> petStorage = new List<Pet>();
    public List<IKeyValuePair<string, Pet[]>> pvpPetTeam = new List<IKeyValuePair<string, Pet[]>>();
    [XmlIgnore] public List<Pet> petDict => petBag.Concat(petStorage).Where(x => x != null).ToList();
    
    [XmlIgnore] public List<Item> itemBag { 
        get => itemStorage.OrderBy(x => x.id).ToList();
        set => itemStorage = value;
    }
    public List<Item> itemStorage = new List<Item>();
    public List<Mail> mailStorage = new List<Mail>();
    public List<Mission> missionStorage = new List<Mission>();
    public List<Activity> activityStorage = new List<Activity>();
    public List<BattleRecord> battleRecordStorage = new List<BattleRecord>();

    public SettingsData settingsData;
    public YiTeRogueData yiteRogueData;

    [XmlIgnore] public NoobCheckPoint noobCheckPoint => NoobController.noobCheckPoint;
    [XmlIgnore] public int initMapId => noobCheckPoint switch {
        NoobCheckPoint.PetBag   => 71,
        NoobCheckPoint.Map      => 71,
        NoobCheckPoint.Train    => 61,
        NoobCheckPoint.Battle   => 61,
        _ => settingsData.initMapId,
    };

    public GameData() {
        InitGameData();
    }

    public void InitGameData() {
        version = string.Empty;
        firstLoginDate = DateTime.Now;
        lastLoginDate = DateTime.Now;
        achievement = 0;

        gender = false;
        nickname = string.Empty;
        petBag = new Pet[6];
        petStorage = new List<Pet>();
        pvpPetTeam = new List<IKeyValuePair<string, Pet[]>>();
        itemStorage = new List<Item>();
        mailStorage = new List<Mail>();
        missionStorage = new List<Mission>();
        activityStorage = new List<Activity>();
        battleRecordStorage = new List<BattleRecord>();

        settingsData = new SettingsData();
        yiteRogueData = new YiTeRogueData();
    }

    public static GameData GetDefaultData(int initCoin = 2000, int initDiamond = 0) {
        GameData gameData = new GameData();
        gameData.petBag = new Pet[6] {
            new Pet(1, 10),
            new Pet(4, 10),
            new Pet(7, 10),
            new Pet(10001, 10),
            new Pet(10004, 10),
            new Pet(10007, 10),
        };
        gameData.itemStorage.Add(new Item(Item.COIN_ID, initCoin));
        gameData.itemStorage.Add(new Item(Item.DIAMOND_ID, initDiamond));
        gameData.itemStorage.Add(new Item(10237, 1));
        gameData.missionStorage.Add(new Mission(1));
        return gameData;
    }

    public bool IsEmpty() {
        return string.IsNullOrEmpty(nickname);
    }

    public bool IsNoob() {
        return NoobController.GetNoobCheckPoint() != NoobCheckPoint.Max;
    }

}
