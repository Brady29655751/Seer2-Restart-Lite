using System.Xml.Serialization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapEntities
{
    [XmlElement("teleport")] public List<TeleportInfo> teleports;
    [XmlElement("npc")] public List<NpcInfo> npcs;
    [XmlElement("farm")] public List<NpcInfo> farms;
    [XmlElement("animal")] public List<NpcInfo> animals;
    [XmlElement("pond")] public List<NpcInfo> ponds;
    [XmlElement("insect")] public List<NpcInfo> insects;
    [XmlElement("nest")] public List<NpcInfo> nests;
}
