using System;
using System.Xml.Serialization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PetExp
{
    [XmlAttribute] public int id;
    public int level;   // 等級
    public uint totalExp;   // 目前總計獲得EXP
    
    [XmlIgnore] public PetExpInfo info => Database.instance.GetPetInfo(id)?.exp;
    public int expType => info.expType;
    public int evolveLevel => info.evolveLevel;
    public uint levelUpExp => (PetExpSystem.GetTotalExp(level + 1, expType) - totalExp);    // 距離升級所需EXP
    public uint maxExp => (PetExpSystem.GetTotalExp(100, expType));

    public PetExp() {}

    public PetExp(int _id, int _level, uint exp = 0) {
        id = _id;
        level = _level;
        totalExp = (exp == 0) ? PetExpSystem.GetTotalExp(_level, expType) : exp;
    }

    public PetExp(PetExp rhs) {
        id = rhs.id;
        level = rhs.level;
        totalExp = rhs.totalExp;
    }

    /// <summary>
    /// Let this pet gain exp. Return whether it should evolve.
    /// </summary>
    /// <param name="exp">Gain how much exp</param>
    /// <returns>evolve or not</returns>
    public bool GainExp(uint exp) {
        if (level == 100)
            return false;

        if (exp < levelUpExp) {
            totalExp += exp;
            return false;
        }

        while ((exp >= levelUpExp) && (level < 100)) {
            exp -= levelUpExp;
            totalExp += levelUpExp;
            level++;
        }
        return (level >= evolveLevel) && (info.evolvePetId != 0) && (info.evolveLevel != 0);
    }

}
