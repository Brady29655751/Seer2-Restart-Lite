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
            YiTeRogueEventType.BattleEasy   => GetBattleChoiceList(choiceEvent),
            YiTeRogueEventType.BattleHard   => GetBattleChoiceList(choiceEvent),
            YiTeRogueEventType.End          => GetBattleChoiceList(choiceEvent),
            _ => new List<YiTeRogueChoice>() { YiTeRogueChoice.Default },
        };
    }

    private static List<YiTeRogueChoice> GetStartChoiceList(YiTeRogueEvent choiceEvent) {
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
                int index = YiTeRogueData.instance.petBag.IndexOf(null);
                Player.instance.gameData.yiteRogueData.petBag[index] = pet;
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
                    return new YiTeRogueChoice(item.name + "\n" + item.info.effectDescription, () => {
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
                    Item.OpenHintbox(endPrize).SetOptionCallback(() => TeleportHandler.Teleport(500));
                }, "item[5]").SingleToList();
        }
    }
}