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

        switch (choiceEvent.type) {
            default:    
                return new List<YiTeRogueChoice>(){ YiTeRogueChoice.Default };
            case YiTeRogueEventType.Start:
                return GetStartChoiceList(choiceEvent);
        }
    }

    private static List<YiTeRogueChoice> GetStartChoiceList(YiTeRogueEvent choiceEvent) {
        var petIds = choiceEvent.GetData("pet")?.ToIntList('/');
        var petLevel = (int)Identifier.GetNumIdentifier(choiceEvent.GetData("level", "60"));
        if (petIds == null)
            return new List<YiTeRogueChoice>(){ YiTeRogueChoice.Default };
        
        Pet GetStartPet(int id) 
        {
            var pet = new Pet(id, petLevel);
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
}