using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ExitGames.Client.Photon.StructWrapping;
using UnityEngine;
using Random = UnityEngine.Random;

public class SpriteKingPanel : Panel
{
    [SerializeField] private string kingActivityId = "30001", soulActivityId = "soul";
    [SerializeField] private List<PetSelectBlockView> selectBlockViews;

    private Activity kingActivity => Activity.Find(kingActivityId);
    private Activity soulActivity => Activity.Find(soulActivityId);
    private List<int> soulPetId => new List<int>{ 981, 982, 983, 984, 990, 985, 986, 987, 988 };
    private new string light => soulActivity.GetData($"light[north]", Enumerable.Repeat("0", soulPetId.Count).ConcatToString(string.Empty));
    private const int ITEM_ID = 400016;

    public override void Init()
    {
        base.Init();
        ShowLight();
    }

    public void GetKing()
    {
        if (soulActivity.GetData<bool>("done[north][10]", "false"))
        {
            Hintbox.OpenHintboxWithContent("你已经获得过约瑟了哦！\n查看精灵背包或仓库吧！", 16);
            return;    
        }

        if (light.Contains('0'))
        {
            var item =  Item.Find(ITEM_ID);
            if ((item == null) || (item.num < 50))
            {
                Hintbox.OpenHintboxWithContent("头像全部点亮或集齐50个<color=#ffbb33>【希望之光】</color>才能获得约瑟！请查看<color=#ffbb33>【挑战规则】</color>！", 16);
                return;
            }

            Item.Remove(ITEM_ID, 50);
        }

        var pet = new Pet(990, 105);
        Pet.Add(pet);
        Pet.OpenHintbox(pet);

        soulActivity.SetData("light[north]", Enumerable.Repeat("1", soulPetId.Count).ConcatToString(string.Empty));
        soulActivity.SetData("done[north][10]", "true");
        SaveSystem.SaveData();

        ShowLight();
    }

    public void ShowLight()
    {
        for(int i = 0; i < selectBlockViews.Count; i++)
        {
            var l = soulActivity.GetData($"light[north]", Enumerable.Repeat("0", soulPetId.Count).ConcatToString(string.Empty))[i];
            selectBlockViews[i].SetPet((l == '0') ? null : Pet.GetExamplePet(soulPetId[i]));
        }
    }

    public void Fight(string type)
    {
        
        if (Enumerable.Range(1, 8).Any(x => !soulActivity.GetData<bool>($"done[north][{x}]", "false")))
        {
            Hintbox.OpenHintboxWithContent("先战胜所有灵兽再来挑战吧！", 16);
            return;
        }
        

        if (kingActivity.GetData<bool>($"done[{type}]", "false"))
        {
            var hintbox = Hintbox.OpenHintboxWithContent("今日已通过此关卡，确定要再次挑战吗？\n<color=#ffbb33>每日仅首次通关有奖励！</color>" , 14);
            hintbox.SetOptionNum(2);
            hintbox.SetOptionCallback(() => OnConfirmFight(type));
            return;
        }

        OnConfirmFight(type);
    }

    private void OnConfirmFight(string type)
    {
        switch (type)
        {
            case "6v6":
                Fight6v6();
                return;

            case "2v2":
                Fight2v2();
                return;

            case "1v1":
                Fight1v1();
                return;
        }
    }

    private void OnFightWin(string type)
    {
        if (kingActivity.GetData<bool>($"done[{type}]", "false"))
            return;

        if (light.Contains('0'))
        {
            var info = new List<string>();
            var newLight = light;
            var ignite = Enumerable.Range(0, soulPetId.Count).ToList().Random(3, false);

            foreach (var index in ignite)
            {
                var l = newLight[index];
                var action = l == '0' ? "点亮" : "熄灭";
                var petName = Pet.GetPetInfo(soulPetId.Get(index, 990))?.name;

                newLight = newLight.Remove(index, 1).Insert(index, (l == '0') ? "1" : "0");
                info.Add($"{action}了<color=#ffbb33>【{petName}】</color>！");
            }

            soulActivity.SetData("light[north]", newLight);
            ShowLight();

            Hintbox.OpenHintboxWithContent(info.ConcatToString("\n"), 14);
        }

        var item = new Item(ITEM_ID);
        Item.Add(item);
        Item.OpenHintbox(item);

        kingActivity.SetData($"done[{type}]", "true");
        SaveSystem.SaveData();
    }

    private void Fight6v6()
    {
        var settings = new BattleSettings()
        {
            petCount = 6,
            mode = BattleMode.SPT,
            starLimit = 4,
        };

        var pets = new int[] { 981, 982, 983, 984, 987, 988 };
        var hps = new int[] { 30000, 25000, 25000, 25000, 65000, 30000 };
        var superSkillIds = new int[] { 10498, 10759, 11049, 11371, 12181, 10628 };
        var loopSkillIds = new string[]
        {
            "10493,10490,10495,10491,10500",
            "10753,10754,10749,10761,10756",
            "11041,11052,11045,11042",
            "11368,11366,11365,11373",
            "12172,12176,12174,12184,12180",
            "10619,10620,10624,10627,10629",
        };

        var boss = pets.Select((x, i) => 
        {
            return new BossInfo()
            {
                petId = pets[i],
                level = 105,
                status = new BattleStatus(){ hp = hps[i] },
                initBuffIds = "13,14,17,99",
                loopSkillIds = loopSkillIds[i],
                superSkillId = superSkillIds[i],
            };
        }).ToList();


        var battle = new BattleInfo()
        {
            id = "6v6",
            settings = settings,
            enemyInfo = boss,
            winHandler = NpcButtonHandler.Callback(() => OnFightWin("6v6")).SingleToList(),
        };

        Battle.StartBattle(battle);
    }

    private void Fight2v2()
    {
        var settings = new BattleSettings()
        {
            petCount = 2,
            parallelCount = 2,
            mode = BattleMode.SPT,
            starLimit = 4,
        };

        var boss1 = new BossInfo()
        {
            petId = 985,
            level = 105,
            status = new BattleStatus(){ hp = 35000 },
            initBuffIds = "-8",
            loopSkillIds = "13023,13020,13030,13020,13023,13030,13020",
            superSkillId = 13029,
        };

        var boss2 = new BossInfo()
        {
            petId = 986,
            level = 105,
            status = new BattleStatus(){ hp = 35000 },
            initBuffIds = "-8",
            loopSkillIds = "13037,13040,13039,13041,13037,13040,13039,13037,13041",
            superSkillId = 13046,
        };

        var battle = new BattleInfo()
        {
            id = "2v2",
            settings = settings,
            enemyInfo = new List<BossInfo>(){ boss1, boss2 },
            winHandler = NpcButtonHandler.Callback(() => OnFightWin("2v2")).SingleToList(),
        };

        Battle.StartBattle(battle);
    }

    private void Fight1v1()
    {
        var settings = new BattleSettings()
        {
            petCount = 1,
            mode = BattleMode.Special,
            starLimit = 6,
        };

        var boss = new BossInfo()
        {
            petId = 990,
            level = 105,
            status = new BattleStatus(){ hp = 135000 },
            initBuffIds = "-8,-3088",
            loopSkillIds = "13134,13135,13136",
            superSkillId = 13133,
        };

        var battle = new BattleInfo()
        {
            id = "1v1",
            settings = settings,
            enemyInfo = boss.SingleToList(),
            winHandler = NpcButtonHandler.Callback(() => OnFightWin("1v1")).SingleToList(),
        };

        Battle.StartBattle(battle);
    }

    public void Shop()
    {
        var panel = Panel.OpenPanel<ItemShopPanel>();
        panel.SetPanelIdentifier("item", "500811/490990/" + Enumerable.Range(21013, 8).Select(x => x.ToString()).ConcatToString("/"));
        panel.SetPanelIdentifier("limit", "1/1/" + Enumerable.Repeat("-1", 8).ConcatToString("/"));
        panel.SetPanelIdentifier("currency", "400016/2");
    }
}
