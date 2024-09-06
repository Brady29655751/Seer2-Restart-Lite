using System;
using System.Xml.Serialization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class PetBasic
{
    public int baseId => info.baseId;

    [XmlAttribute] public int id;
    public Personality personality;
    public int height, weight;
    public DateTime getPetDate;

    [XmlIgnore] public PetBasicInfo info => Database.instance.GetPetInfo(id)?.basic;

    public PetBasic()
    {
    }

    public PetBasic(int _id, int p = -1)
    {
        id = _id;
        personality = (Personality)((p == -1) ? Random.Range(0, 25) : p);
        height = info.baseHeight + Random.Range(0, 6);
        weight = info.baseWeight + Random.Range(0, 6);
        getPetDate = DateTime.Now;
    }

    public PetBasic(PetBasic rhs)
    {
        id = rhs.id;
        personality = rhs.personality;
        height = rhs.height;
        weight = rhs.weight;
        getPetDate = rhs.getPetDate;
    }

    public void ToBestPersonality()
    {
        personality = (info.baseStatus.atk >= info.baseStatus.mat) ? Personality.固执 : Personality.保守;
    }

    public int uid
    {
        get
        {
            unchecked // 允许溢出
            {
                int hash = 17;
                hash = hash * 31 + this.id;
                hash = hash * 31 + this.height;
                hash = hash * 31 + this.weight;
                hash = hash * 31 + (int)this.personality;
                hash = hash * 31 + this.getPetDate.GetHashCode();
                return hash;
            }
        }
    }
}

public enum Personality
{
    // 物攻-, 特攻-, 物防-, 特防-, 速度-
    实干,
    固执,
    孤独,
    调皮,
    勇敢, // 物攻+ 
    保守,
    害羞,
    稳重,
    马虎,
    冷静, // 特攻+
    大胆,
    顽皮,
    坦率,
    无虑,
    悠闲, // 物防+
    沉着,
    慎重,
    温顺,
    浮躁,
    狂妄, // 特防+
    胆小,
    开朗,
    急躁,
    天真,
    认真, // 速度+
}