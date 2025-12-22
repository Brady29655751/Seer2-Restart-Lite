using System;
using System.Linq;
using System.Xml.Serialization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NpcButtonHandler
{
    public string description;

    [XmlAttribute("type")] public string typeId;
    public ButtonEventType type => typeId?.ToButtonEventType() ?? ButtonEventType.None;

    [XmlElement("action")] public string actionType;
    public NpcAction action => actionType?.ToNpcAction() ?? NpcAction.None;

    [XmlElement("condition")] public List<string> condition;
    [XmlElement("param")] public List<string> param;

    [XmlIgnore] public List<Action> callback = null;

    public static NpcButtonHandler Callback(Action callback) {
        return new NpcButtonHandler(){ 
            callback = new List<Action>(){ callback } 
        };
    }

    public NpcButtonHandler() {}

    public NpcButtonHandler(NpcButtonHandler rhs) {
        description = rhs.description;
        typeId = rhs.typeId;
        actionType = rhs.actionType;
        condition = rhs.condition.ToList();
        param = rhs.param.ToList();
        callback = rhs.callback;
    }

}

public static class NpcActionDatabase {

    private static Dictionary<string, ButtonEventType> buttonEventTypeDict = new Dictionary<string, ButtonEventType>() {
        { "auto", ButtonEventType.Auto },
        { "click", ButtonEventType.OnPointerClick },
        { "enter", ButtonEventType.OnPointerEnter },
        { "exit", ButtonEventType.OnPointerExit },
        { "hover", ButtonEventType.OnPointerOver },
    };

    private static Dictionary<string, NpcAction> npcActionDict = new Dictionary<string, NpcAction>() {
        { "setNpcParam", NpcAction.SetNpcParam },
        { "openHintbox", NpcAction.OpenHintbox },
        { "openPanel", NpcAction.OpenPanel },
        { "openDialog", NpcAction.OpenDialog },
        { "teleport", NpcAction.Teleport },
        { "setItem", NpcAction.SetItem },
        { "getPet", NpcAction.GetPet },
        { "removePet", NpcAction.RemovePet },
        { "setPet", NpcAction.SetPet },
        { "evolvePet", NpcAction.EvolvePet },
        { "setMission", NpcAction.SetMission },
        { "setActivity", NpcAction.SetActivity },
        { "battle", NpcAction.Battle },
        { "player", NpcAction.Player },
        { "mail", NpcAction.SetMail },
        { "fish", NpcAction.Fish },
        { "miniGame", NpcAction.MiniGame },
        { "callback", NpcAction.Callback },
    };

    private static List<NpcAction> petActionList = new List<NpcAction>() {
        NpcAction.RemovePet, NpcAction.SetPet, NpcAction.EvolvePet,
    };

    public static ButtonEventType ToButtonEventType(this string type) {
        return buttonEventTypeDict.Get(type, ButtonEventType.None);
    }

    public static NpcAction ToNpcAction(this string type) {
        return npcActionDict.Get(type, NpcAction.None);
    }
}

public enum ButtonEventType {
    None,
    Auto,
    OnPointerClick,
    OnPointerEnter,
    OnPointerExit,
    OnPointerOver,
}

public enum NpcAction
{
    None,
    SetNpcParam,
    OpenHintbox,
    OpenPanel,
    OpenDialog,
    Teleport,
    SetItem,
    GetPet,
    RemovePet,
    SetPet,
    EvolvePet,
    SetMission,
    SetActivity,
    Battle,
    Player,
    SetMail,
    Fish,
    MiniGame,
    Callback,
}