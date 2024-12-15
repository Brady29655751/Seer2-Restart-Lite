using System;
using System.Xml.Serialization;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class YiTeRogueChoice 
{
    public string description;
    public Action callback = null;
    public string iconType = null;
    public Sprite icon => SpriteSet.GetIconSprite(iconType);

    public static YiTeRogueChoice Default => new YiTeRogueChoice("无事发生");

    public YiTeRogueChoice(){}
    public YiTeRogueChoice(string description, Action callback = null, string iconType = null) {
        this.description = description;
        this.callback = callback;
        this.iconType = iconType;
    }

    public static List<YiTeRogueChoice> GetChoiceList(YiTeRogueEvent choiceEvent) {
        if (choiceEvent == null)
            return null;

        return choiceEvent.type switch
        {
            YiTeRogueEventType.Start        => GetStartChoiceList(choiceEvent),
            YiTeRogueEventType.Heal         => GetStartChoiceList(choiceEvent),
            YiTeRogueEventType.BattleEasy   => GetBattleChoiceList(choiceEvent),
            YiTeRogueEventType.BattleHard   => GetBattleChoiceList(choiceEvent),
            YiTeRogueEventType.Reward       => GetDialogChoiceList(choiceEvent),
            YiTeRogueEventType.Dialog       => GetDialogChoiceList(choiceEvent),
            YiTeRogueEventType.End          => GetBattleChoiceList(choiceEvent),
            _ => new List<YiTeRogueChoice>() { YiTeRogueChoice.Default },
        };
    }

    private static List<YiTeRogueChoice> GetStartChoiceList(YiTeRogueEvent choiceEvent) {
        var floor = YiTeRogueData.instance.floor;
        var petBag = YiTeRogueData.instance.petBag;
        var petIds = choiceEvent.GetData("pet")?.ToIntList('/');
        var petLevel = petBag.Min(x => x?.level ?? 60);
        if (petIds == null)
            return new List<YiTeRogueChoice>(){ YiTeRogueChoice.Default };

        Pet GetStartPet(int id) 
        {
            var pet = new Pet(id, petLevel);
            pet.exp.fixedMaxLevel = int.MaxValue;
            foreach (var skill in pet.skills.secretSkill)
                pet.skills.LearnNewSkill(skill);
            return pet;
        }
        YiTeRogueChoice GetStartChoice(Pet pet) 
        {
            return new YiTeRogueChoice(pet.name, () => {
                int index = petBag.IndexOf(null);
                YiTeRogueData.instance.petBag[index] = pet;
                if (choiceEvent.type == YiTeRogueEventType.Heal)
                    foreach (var pet in YiTeRogueData.instance.petBag)
                        pet.currentStatus.hp = pet.normalStatus.hp;
                SaveSystem.SaveData();
            }, "pet[" + pet.id + "]");
        }
        return petIds.Select(GetStartPet).Select(GetStartChoice).ToList();
    }   

    private static List<YiTeRogueChoice> GetBattleChoiceList(YiTeRogueEvent choiceEvent) {
        var mapId = int.Parse(choiceEvent.GetData("map_id", "84"));
        var npcId = int.Parse(choiceEvent.GetData("npc_id", "8401"));
        var battleId = choiceEvent.GetData("battle_id", "21");
        var result = choiceEvent.GetData("result", "none");
        var reward = choiceEvent.GetData("reward", "none").ToIntList('/');
        switch (result) {
            default:
                return new YiTeRogueChoice(npcId + ": " + battleId, () => Map.GetMap(mapId, map => {
                    var petBag = YiTeRogueData.instance.petBag;
                    var battleInfo = Map.GetBattleInfo(map, npcId, battleId)?.FixToYiTeRogue(choiceEvent);
                    if (battleInfo == null) {
                        Hintbox.OpenHintboxWithContent("NPC战斗信息为空", 16);
                        return;
                    }
                    Battle battle = new Battle(battleInfo);
                    SceneLoader.instance.ChangeScene(SceneId.Battle);
                }, (error) => Hintbox.OpenHintboxWithContent(error, 16)), "buff[" + choiceEvent.eventIconBuffId + "]").SingleToList();

            case "win":
                return reward.Select(id => {
                    var item = (id == 5) ? YiTeRogueData.instance.prize : new Item(id, 1);
                    return new YiTeRogueChoice(item.info.effectDescription, () => {
                        if (item.info.type == ItemType.Currency)
                            Item.Add(item);
                        else
                            Item.AddTo(item, YiTeRogueData.instance.itemBag);
                        Item.OpenHintbox(item);
                    }, "item[" + item.info.resId + "]");
                }).ToList();

            case "lose":
                var endPrize = YiTeRogueData.instance.prize;
                return new YiTeRogueChoice("领取奖励", () => {
                    Item.Add(endPrize);
                    Item.OpenHintbox(endPrize).SetOptionCallback(Panel.ClosePanel<YiTeRoguePanel>);
                }, "item[5]").SingleToList();
        }
    }

    private static List<YiTeRogueChoice> GetDialogChoiceList(YiTeRogueEvent choiceEvent) {
        var dialogId = choiceEvent.GetData("dialog_id", "0");
        var npc = Map.GetNpcInfo(Player.instance.currentMap, choiceEvent.npcId);
        var replyHandler = npc?.dialogHandler?.Find(x => x.id == dialogId)?.replyHandler;
        if (replyHandler == null)
            return YiTeRogueChoice.Default.SingleToList();

        var choiceList = new List<YiTeRogueChoice>();
        var choice = YiTeRogueChoice.Default;
        var options = replyHandler.FindAllIndex(x => x.typeId != "branch").Append(replyHandler.Count).ToList();

        for (int i = 0; i < options.Count - 1; i++) {
            int index = options[i], nextIndex = options[i+1];
            var action = Enumerable.Range(index, nextIndex - index).Select(x => NpcHandler.GetNpcEntity(null, replyHandler[x], null)).ToList();
            choiceList.Add(new YiTeRogueChoice(replyHandler[index].description, () => action?.ForEach(x => x?.Invoke())));
        }

        return choiceList;
    }
}