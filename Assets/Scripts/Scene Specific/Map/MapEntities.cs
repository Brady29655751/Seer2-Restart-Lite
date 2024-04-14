using System.Xml.Serialization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapEntities
{
    [XmlElement("teleport")] public List<TeleportInfo> teleports;
    [XmlElement("npc")] public List<NpcInfo> npcs;
}
