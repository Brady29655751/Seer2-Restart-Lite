using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

public class PetResist
{
    public int id;
    public int lockState = -1;
    public int criResist, fixResist, perResist; // 暴伤抗性，固伤抗性，百分比抗性
    public IKeyValuePair<int, int> abnormalResist = new IKeyValuePair<int, int>(0, 0);
    public IKeyValuePair<int, int> unhealthyResist = new IKeyValuePair<int, int>(0, 0);


    private int star => Pet.GetPetInfo(id)?.star ?? 0;
    public int abnormalBuffId => Buff.GetBuffInfo(abnormalResist.key)?.id ?? 199;
    public int unhealthyBuffId => Buff.GetBuffInfo(unhealthyResist.key)?.id ?? 1999;
    public Buff abnormalBuff => new Buff(abnormalBuffId);
    public Buff unhealthyBuff => new Buff(unhealthyBuffId);
    [XmlIgnore] public List<Buff> resistBuffs => GetResistBuffs();  // 戰鬥時添加的初始抗性印記


    public PetResist(){}

    public PetResist(int id)
    {
        this.id = id;
        lockState = (star < 6) ? -1 : 0;
        criResist = 0;
        fixResist = 0;
        perResist = 0;
        abnormalResist = new IKeyValuePair<int, int>(0, 0);
        unhealthyResist = new IKeyValuePair<int, int>(0, 0);
    }

    public PetResist(PetResist rhs)
    {
        id = rhs.id;
        lockState = rhs.lockState;
        criResist = rhs.criResist;
        fixResist = rhs.fixResist;
        perResist = rhs.perResist;
        abnormalResist = new IKeyValuePair<int, int>(rhs.abnormalResist);
        unhealthyResist = new IKeyValuePair<int, int>(rhs.unhealthyResist);
    }

    public PetResist(Dictionary<string, int> dict)
    {
        SetResistDict(dict);
    }

    public List<Buff> GetResistBuffs()
    {
        if (lockState <= 0)
            return new List<Buff>();

        var resistBuff = new Buff(Buff.BUFFID_RESIST_BUFF);

        resistBuff.SetBuffIdentifier("option[cri]", criResist);
        resistBuff.SetBuffIdentifier("option[fix]", fixResist);
        resistBuff.SetBuffIdentifier("option[per]", perResist);

        resistBuff.SetBuffIdentifier("option[abnormalId]", abnormalBuffId);
        resistBuff.SetBuffIdentifier("option[unhealthyId]", unhealthyBuffId);
        resistBuff.SetBuffIdentifier("option[abnormalValue]", abnormalResist?.value ?? 0);
        resistBuff.SetBuffIdentifier("option[unhealthyValue]", unhealthyResist?.value ?? 0);

        return new List<Buff>(){ resistBuff };
    }

    public Dictionary<string, int> ToDictionary()
    {
        return new Dictionary<string, int>()
        {
            {"critical", criResist},
            {"fix", fixResist},
            {"percentage", perResist},
            {"abnormal.id", abnormalResist.key},
            {"unhealthy.id", unhealthyResist.key},
            {"abnormal.value", abnormalResist.value},
            {"unhealthy.value", unhealthyResist.value},
        };
    }

    public void SetResistDict(Dictionary<string, int> dict)
    {
        if (dict == null)
            return;

        foreach (var entry in dict)
        {
            SetResistValue(entry.Key, entry.Value);
        }
    }

    public void SetResistValue(string type, int value)
    {
        switch (type)
        {
            default:
                return;

            case "critical":
                criResist = value;
                return;

            case "fix":
                fixResist = value;
                return;

            case "percentage":
                perResist = value;
                return;

            case "abnormal.id":
                abnormalResist.key = value;
                return;

            case "unhealthy.id":
                unhealthyResist.key = value;
                return;

            case "abnormal.value":
                abnormalResist.value = value;
                return;

            case "unhealthy.value":
                unhealthyResist.value = value;
                return;
        }
    }

    public void ToBest()
    {
        lockState = (star < 6) ? -1 : 1;
        criResist = fixResist = perResist = 35;
        abnormalResist.value = unhealthyResist.value = 55;
    }

}
