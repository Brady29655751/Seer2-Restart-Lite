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
            YiTeRogueEventType.Store        => GetStoreChoiceList(choiceEvent),
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
        var petLevel = petBag.Where(x => x != null).DefaultIfEmpty(Pet.GetExamplePet(91)).Min(x => x.level);
        if (ListHelper.IsNullOrEmpty(petIds))
            return new YiTeRogueChoice(choiceEvent.content, () => {
                foreach (var pet in YiTeRogueData.instance.petBag) {
                    if (pet == null)
                        continue;
                    pet.currentStatus.hp = pet.normalStatus.hp;
                }
            }, "buff[" + choiceEvent.eventIconBuffId + "]").SingleToList();

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
                    foreach (var pet in YiTeRogueData.instance.petBag) {
                        if (pet == null)
                            continue;
                        pet.currentStatus.hp = pet.normalStatus.hp;
                    }
                else {
                    var yiteActivity = Activity.Find("yite_rogue");
                    var initSkillId = yiteActivity.GetData("init_skill[" + pet.id + "]", "none").ToIntList('/');
                    if (!ListHelper.IsNullOrEmpty(initSkillId)) {
                        pet.normalSkill = initSkillId.Take(4).Select(x => Skill.GetSkill(x, false)).ToArray();
                        pet.superSkill = Skill.GetSkill(initSkillId.Last(), false);
                    }
                }
                SaveSystem.SaveData();
            }, "pet[" + pet.id + "]");
        }
        return petIds.Select(GetStartPet).Select(GetStartChoice).ToList();
    }   

    private static List<YiTeRogueChoice> GetStoreChoiceList(YiTeRogueEvent choiceEvent) {
        var floor = YiTeRogueData.instance.floor;
        var pet = YiTeRogueData.instance.petBag.First();
        var itemList = choiceEvent.GetData("item", "none");
        return new YiTeRogueChoice("神秘商店", () => {
            var panel = Panel.OpenPanel<ItemShopPanel>();
            panel.SetPanelIdentifier("title", "神秘商店"); 
            panel.SetShopMode(ItemShopMode.BuyYiTe);
            panel.SetPanelIdentifier("item", itemList);
            panel.SetCurrencyType(6, 0);

            // Record current yite skill. Auto load it next time.
            if ((floor == 0) && (choiceEvent.step == 1) && (pet.basic.baseId == 91)) {
                Activity.Find("yite_rogue").SetData("init_skill[" + pet.id + "]", pet.skills.normalSkillId
                    .Append(pet.skills.superSkillId).Select(x => x.ToString()).ConcatToString("/"));
            }
        }, "buff[" + choiceEvent.eventIconBuffId + "]").SingleToList();
    }

    private static List<YiTeRogueChoice> GetBattleChoiceList(YiTeRogueEvent choiceEvent) {
        var mapId = int.Parse(choiceEvent.GetData("map_id", "84"));
        var npcId = int.Parse(choiceEvent.GetData("npc_id", "8401"));
        var battleId = choiceEvent.GetData("battle_id", "21");
        var battleNum = int.Parse(battleId);
        var result = choiceEvent.GetData("result", "none");
        var reward = choiceEvent.GetData("reward", "none").Split('/');
        var difficulty = YiTeRogueData.instance.difficulty;

        var petData = GameManager.versionData.petData;
        var petDict = (difficulty == YiTeRogueMode.Mod) ? petData.petModLastEvolveDictionary : petData.petLastEvolveDictionary;

        BattleInfo GetBattleInfo(Map map) {
            var normalEnemyList = petDict.Where(x => x.element != Element.精灵王).ToList();
            if (mapId == 82) {
                var bossId = battleNum switch {
                    7   => 990,
                    14  => 1990,
                    21  => 989,
                    _   => normalEnemyList.Random().id,
                };
                var randomEnemyInfo = BossInfo.GetRandomEnemyInfo(bossId.SingleToList());
                return new BattleInfo(){ enemyInfo = randomEnemyInfo.SingleToList() };
            }
            if (mapId >= 85) {
                var bossId = (battleNum % 7 == 0) ? new List<int>(){ 990, 1990, 989, 990, 1990, 989 } 
                    : normalEnemyList.Random(6, false).Select(x => x.id).ToList();
                var randomEnemyInfo = bossId.Select(x => BossInfo.GetRandomEnemyInfo(x.SingleToList())).ToList();
                return new BattleInfo(){ enemyInfo = randomEnemyInfo };
            }
            return Map.GetBattleInfo(map, npcId, battleId);
        }

        switch (result) {
            default:
                var map = Map.GetMap(mapId);
                var battleInfo = GetBattleInfo(map)?.FixToYiTeRogue(choiceEvent);
                var enemyId = battleInfo?.enemyInfo?.FirstOrDefault()?.petId;
                var description = ((enemyId == null) || ((mapId == 82) && (battleNum % 7 != 0))) ? "随机的对手" : 
                    battleInfo.enemyInfo.Select(x => Pet.GetPetInfo(x.petId)?.name).ConcatToString("、");
                var icon = (enemyId == null) ? ("buff[" + choiceEvent.eventIconBuffId + "]") : ("pet[" + enemyId + "]");
                return new YiTeRogueChoice(description, () => {
                    if (battleInfo == null) {
                        Hintbox.OpenHintboxWithContent("NPC战斗信息为空", 16);
                        return;
                    }
                    Battle battle = new Battle(battleInfo);
                    SceneLoader.instance.ChangeScene(SceneId.Battle);
                }, icon).SingleToList();

            case "win":
                return reward.Select(expr => {
                    var type = expr.Substring(0, expr.IndexOf('['));
                    var id = int.Parse(expr.TrimParentheses());
                    switch (type) {
                        default:
                            return YiTeRogueChoice.Default;
                        case "item":
                            var item = (id == 5) ? YiTeRogueData.instance.prize : new Item(id, 1);
                            return new YiTeRogueChoice(item.info.effectDescription, () => {
                                if (item.info.type == ItemType.Currency)
                                    Item.Add(item);
                                else
                                    Item.AddTo(item, YiTeRogueData.instance.itemBag);

                                Item.OpenHintbox(item);
                                if (id == 5)
                                    Hintbox.OpenHintboxWithContent("恭喜你通过第 " + YiTeRogueEvent.GetEndFloorByDifficulty(difficulty) + " 层，完成挑战！", 16);
                            }, "item[" + item.info.resId + "]");
                        case "buff":
                            var buff = new Buff(id);
                            var endlId = buff.description.IndexOf("\n");
                            var buffDesc = (endlId < 0) ? buff.description : buff.description.Substring(endlId + 1);
                            return new YiTeRogueChoice(buffDesc, () => {
                                if (buff.IsYiTeMedicineItem()) {
                                    var statusId = id - 400000;
                                    YiTeRogueData.instance.buffStatus[statusId % 5] += statusId / 5 * 50;
                                } else
                                    YiTeRogueData.instance.buffIds.Add(id);
                                
                                var nextEventList = choiceEvent.nextEventList?.Where(x => x.battleDifficulty >= 0).ToList();
                                if (ListHelper.IsNullOrEmpty(nextEventList))
                                    return;

                                nextEventList.ForEach(x => x.SetData("reward", YiTeRogueEvent.GetYiTeRogueEventData(x.type)
                                    ?.Find(x => x.key == "reward")?.value ?? x.GetData("reward")));
                                SaveSystem.SaveData();
                            }, "buff[" + buff.info.resId + "]");
                    }
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