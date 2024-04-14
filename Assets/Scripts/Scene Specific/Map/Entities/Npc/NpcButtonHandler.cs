using System;
using System.Xml.Serialization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NpcButtonHandler
{
    public string description;

    [XmlAttribute("type")] public string typeId;
    public ButtonEventType type => typeId.ToButtonEventType();

    [XmlElement("action")] public string actionType;
    public NpcAction action => actionType.ToNpcAction();

    [XmlElement("condition")] public List<string> condition;
    [XmlElement("param")] public List<string> param;

}

public static class NpcActionDatabase {

    private static Dictionary<string, ButtonEventType> buttonEventTypeDict = new Dictionary<string, ButtonEventType>() {
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
        { "setPet", NpcAction.SetPet },
        { "evolvePet", NpcAction.EvolvePet },
        { "setMission", NpcAction.SetMission },
        { "setActivity", NpcAction.SetActivity },
        { "battle", NpcAction.Battle },
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
    OnPointerClick,
    OnPointerEnter,
    OnPointerExit,
    OnPointerOver,
}

public enum NpcAction {
    None,
    SetNpcParam,
    OpenHintbox,
    OpenPanel,
    OpenDialog,
    Teleport,
    SetItem,
    GetPet,
    SetPet,
    EvolvePet,
    SetMission,
    SetActivity,
    Battle,
}