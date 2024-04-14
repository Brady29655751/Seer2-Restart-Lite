using System.Xml.Serialization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleInfo 
{
    [XmlAttribute("id")] public string id;
    public string content = string.Empty;
    public BattleSettings settings = new BattleSettings();

    [XmlArray("player"), XmlArrayItem(typeof(BossInfo), ElementName = "pet")] 
    public List<BossInfo> playerInfo; 

    [XmlArray("enemy"), XmlArrayItem(typeof(BossInfo), ElementName = "pet")] 
    public List<BossInfo> enemyInfo;
    
    [XmlArray("winHandler"), XmlArrayItem(typeof(NpcButtonHandler), ElementName = "button")] 
    public List<NpcButtonHandler> winHandler;

    [XmlArray("loseHandler"), XmlArrayItem(typeof(NpcButtonHandler), ElementName = "button")] 
    public List<NpcButtonHandler> loseHandler;
    
}
