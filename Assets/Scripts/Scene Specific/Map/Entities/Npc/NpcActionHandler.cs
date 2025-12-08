using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class NpcActionHandler
{
    public static void SetNpcParam(NpcController npc, NpcButtonHandler handler, Dictionary<int, NpcController> npcList)
    {
        if ((handler.param == null) || (handler.param.Count < 2))
            return;

        int id = int.Parse(handler.param[0]);
        var toNpc = npcList.Get(id, npc);

        for (int i = 1; i < handler.param.Count; i++)
        {
            var option = handler.param[i].Split('=');
            var expr = string.Empty;
            switch (option[0])
            {
                default:
                    break;
                case "active":
                    toNpc.SetActive(bool.Parse(option[1]));
                    break;
                case "target":
                    toNpc.SetRaycastTarget(bool.Parse(option[1]));
                    break;
                case "name":
                    toNpc.SetName(option[1].TryTrimStart("[expr]", out expr) ? Parser.ParseOperation(expr).ToString() : option[1]);
                    break;
                case "sprite":
                    var path = option[1].TrimEnd(".png").Split('/').Select(x => x.TryTrimStart("[expr]", out expr) ?
                        Parser.ParseOperation(expr).ToString() : x).ConcatToString("/");
                    toNpc.SetIcon(path);
                    break;
                case "color":
                    toNpc.SetColor(option[1].ToColor(Color.white));
                    break;
                case "bgm":
                    toNpc.SetBGM(option[1]);
                    break;
                case "namePos":
                    toNpc.SetNamePos(option[1].ToVector2());
                    break;
                case "pos":
                    toNpc.SetPosition(option[1].ToVector2());
                    break;
            }
        }
    }


    public static void SetPlayer(NpcButtonHandler handler)
    {
        if (handler.param == null)
            return;

        for (int i = 0; i < handler.param.Count; i++)
        {
            var option = handler.param[i].Split('=');
            switch (option[0])
            {
                default:
                    break;
                case "sprite":
                    PlayerController.instance.SetPlayerSprite(option[1]);
                    break;
            }
        }
    }

    public static void OpenHintbox(NpcController npc, NpcButtonHandler handler, Dictionary<int, NpcController> npcList)
    {
        if ((handler.param == null) || (handler.param.Count == 0))
            return;

        var type = handler.param[0].Split('=');

        Hintbox hintbox = type[1] switch
        {
            "Item" => Hintbox.OpenHintbox<ItemHintbox>(),
            "PetSelect" => Hintbox.OpenHintbox<PetSelectHintbox>(),
            _ => Hintbox.OpenHintbox()
        };

        if (type[1] == "PetSelect")
            ((PetSelectHintbox)hintbox).SetStorage(Player.instance.petBag.ToList());

        if (handler.param.Count <= 1)
            return;

        for (int i = 1; i < handler.param.Count; i++)
        {
            var option = handler.param[i].Split('=');
            if (option[0] == "callback")
            {
                Action callback = () =>
                {
                    npc?.GetInfo()?.callbackHandler?.FindAll(x => x.typeId == option[1])?.ForEach(x =>
                    {
                        NpcHandler.GetNpcEntity(npc, x, npcList)?.Invoke();
                    });
                };
                hintbox.SetOptionCallback(callback);
            }
            else
                hintbox.SetPanelIdentifier(option[0], option[1]);
        }
    }

    public static void OpenPanel(NpcButtonHandler handler)
    {
        if ((handler.param == null) || (handler.param.Count == 0))
            return;

        var panel = Panel.OpenPanel(handler.param[0]);
        if (handler.param.Count <= 1)
            return;

        for (int i = 1; i < handler.param.Count; i++)
        {
            var option = handler.param[i].Split('=');
            panel.SetPanelIdentifier(option[0], option[1]);
        }

    }

    public static void OpenDialog(NpcInfo npcInfo, NpcButtonHandler handler)
    {
        if ((handler.param == null) || (handler.param.Count == 0))
            return;

        if (handler.param[0] == "null")
        {
            DialogManager.instance.CloseDialog();
            return;
        }
        DialogInfo dialogInfo = npcInfo?.dialogHandler?.Find(x => x.id == handler.param[0]);
        DialogManager.instance.SetCurrentNpc(npcInfo);

        if (dialogInfo?.content.StartsWith("story=") ?? false)
        {
            dialogInfo.rawContent = dialogInfo.rawContent.Substring("story=".Length);
            DialogManager.instance.OpenStoryDialog(dialogInfo);
        }
        else
        {
            DialogManager.instance.OpenDialog(dialogInfo);
        }

    }

    public static void Teleport(NpcButtonHandler handler)
    {
        if ((handler.param == null) || (handler.param.Count == 0))
            return;

        int mapId = int.Parse(handler.param[0]);
        if (handler.param.Count == 1)
        {
            TeleportHandler.Teleport(mapId);
            return;
        }
        TeleportHandler.Teleport(mapId, handler.param[1].ToVector2());
    }

    public static void SetItem(NpcButtonHandler handler)
    {
        if ((handler.param == null) || (handler.param.Count < 2))
            return;

        Action<Item> itemFunc = handler.param[0] switch
        {
            "add" => Item.Add,
            "remove" => (x) => Item.Remove(x.id, x.num),
            "add_to_yite" => (x) => Item.AddTo(x, YiTeRogueData.instance.itemBag),
            "remove_from_yite" => (x) => Item.RemoveFrom(x.id, x.num, YiTeRogueData.instance.itemBag),
            _ => null
        };
        for (int i = 1; i < handler.param.Count; i++)
        {
            var itemInfo = handler.param[i].ToIntList();
            itemFunc?.Invoke(new Item(itemInfo[0], itemInfo[1]));
        }
    }

    public static void GetPet(NpcButtonHandler handler)
    {
        if ((handler.param == null) || (handler.param.Count == 0))
            return;

        Pet pet = null;
        for (int i = 0; i < handler.param.Count; i++)
        {
            var split = handler.param[i].Split('=');
            if (split.Length == 1)
            {
                var petInfo = handler.param[i].ToIntList();
                int count = petInfo.Count;
                int id = petInfo[0];
                int level = (count < 2) ? 1 : petInfo[1];
                bool emblem = (count < 3) ? true : (petInfo[2] != 0);

                pet = new Pet(id, level, emblem);
                Player.instance.gameData.petStorage.Add(pet);
            }
            else
            {
                pet?.SetPetIdentifier(split[0], float.Parse(split[1]));
            }
        }
        SaveSystem.SaveData();
    }

    public static void RemovePet(NpcButtonHandler handler)
    {
        int index = 0;
        if ((handler.param != null) && (handler.param.Count > 0))
        {
            var petInfo = handler.param[0].Split('=');
            if ((petInfo.Length >= 2) && (petInfo[0]?.ToLower() == "index"))
            {
                index = (int)Parser.ParseOperation(petInfo[1]);
                if (!index.IsInRange(0, Player.instance.petBag.Length))
                    return;
            }
        }

        var newPetBag = Player.instance.petBag.ToList();
        newPetBag.RemoveAt(index);
        newPetBag.Add(null);
        Player.instance.petBag = newPetBag.ToArray();
        SaveSystem.SaveData();
    }

    public static void SetPet(NpcButtonHandler handler)
    {
        if ((handler.param == null) || (handler.param.Count == 0))
            return;

        int index = 0;
        for (int i = 0; i < handler.param.Count; i++)
        {
            var petInfo = handler.param[i].Split('=');
            if (petInfo.Length < 2)
                continue;

            if (petInfo[0].ToLower() == "index")
            {
                index = (int)Parser.ParseOperation(petInfo[1]);
                if (!index.IsInRange(0, Player.instance.petBag.Length))
                    return;

                continue;
            }

            Player.instance.gameData.petBag[index].SetPetIdentifier(petInfo[0], float.Parse(petInfo[1]));
        }
        SaveSystem.SaveData();
    }

    public static void EvolvePet(NpcButtonHandler handler)
    {
        if ((handler.param == null) || (handler.param.Count == 0))
            return;

        int index = 0, paramOffset = 0;
        var petInfo = handler.param[0].Split('=');
        if ((petInfo.Length >= 2) && (petInfo[0].ToLower() == "index"))
        {
            paramOffset = 1;
            index = (int)Parser.ParseOperation(petInfo[1]);
            if (!index.IsInRange(0, Player.instance.petBag.Length))
                return;
        }

        int evolveId = (int)Identifier.GetNumIdentifier(handler.param[paramOffset]);
        Player.instance.gameData.petBag[index].EvolveTo(evolveId);
        SaveSystem.SaveData();
    }

    public static void SetMission(NpcButtonHandler handler)
    {
        if ((handler.param == null) || (handler.param.Count < 2))
            return;

        int id = int.Parse(handler.param[0]);
        switch (handler.param[1])
        {
            case "start":
                Mission.Start(id);
                break;
            case "complete":
                Mission.Complete(id);
                break;
            case "checkpoint":
                Mission.Checkpoint(id, handler.param[2]);
                break;
        }
        SaveSystem.SaveData();
    }

    public static void SetActivity(NpcButtonHandler handler)
    {
        if ((handler.param == null) || (handler.param.Count < 1))
            return;

        var activity = Activity.Find(handler.param[0]);
        for (int i = 1; i < handler.param.Count; i++)
        {
            var option = handler.param[i].Split('=');
            activity.SetData(option[0], option[1]);
        }
        SaveSystem.SaveData();
    }

    public static void StartBattle(NpcInfo npcInfo, NpcButtonHandler handler)
    {
        if ((handler.param == null) || (handler.param.Count == 0))
            return;

        string id = handler.param[0];
        Player.instance.currentNpcId = (npcInfo == null) ? 0 : npcInfo.id;
        BattleInfo battleInfo = npcInfo.battleHandler.Find(x => x.id == handler.param[0]);

        if (battleInfo == null)
        {
            if (handler.param.Count <= 1)
                return;

            Hintbox hintbox = Hintbox.OpenHintbox();
            hintbox.SetTitle("提示");
            hintbox.SetContent(handler.param[1], 14, FontOption.Arial);
            hintbox.SetOptionNum(1);
            return;
        }

        Battle.StartBattle(battleInfo);
    }

    public static void SetMail(NpcButtonHandler handler)
    {
        if ((handler.param == null) || (handler.param.Count == 0))
            return;

        var mail = new Mail() { date = DateTime.Now };

        for (int i = 0; i < handler.param.Count; i++)
        {
            var option = handler.param[i].Split('=');
            switch (option[0])
            {
                default:
                    break;
                case "from":
                    mail.sender = option[1];
                    break;
                case "title":
                    mail.title = option[1];
                    break;
                case "content":
                    mail.content = option[1];
                    break;
                case "item":
                    mail.items = option[1].Split('/').Select(x =>
                    {
                        var itemInfo = x.ToIntList();
                        return new Item(itemInfo[0], itemInfo[1]);
                    }).ToList();
                    break;
            }
        }
        Mail.Add(mail);
    }

    public static void Fish()
    {
        int itemId;
        List<int> probability;

        switch (Player.instance.currentMap.categoryId)
        {
            default:
                if (Player.instance.gameData.achievement != 7004)
                    return;

                probability = new List<int>() { 25, 40, 15, 10, 7, 3 };
                itemId = Enumerable.Range(1005, 6).ToList().Random(probability);
                break;
            case 4:
                if (Player.instance.gameData.achievement != 7005)
                    goto default;

                probability = new List<int>() { 25, 40, 15, 10, 7, 3 };
                itemId = Enumerable.Range(1011, 6).ToList().Random(probability);
                break;
        }
        var fish = new Item(itemId, 1);
        Item.Add(fish);
        Item.OpenHintbox(fish);
    }

    public static void MiniGame(NpcButtonHandler handler)
    {
        if (ListHelper.IsNullOrEmpty(handler.param))
            return;

        Player.instance.currentMiniGame = new Activity(handler.param[0]);
        SceneLoader.instance.ChangeScene(SceneId.Game);
    }
}
