using System;
using System.Xml.Serialization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PetExp
{
    [XmlAttribute] public int id;
    public int level;   // 等級
    public int fixedMaxLevel; // 最高等級 (默認 100)
    public uint totalExp;   // 目前總計獲得EXP
    
    [XmlIgnore] public PetExpInfo info => Database.instance.GetPetInfo(id)?.exp;
    public int expType => info.expType;
    public int evolveLevel => info.evolveLevel;
    public uint levelUpExp => (PetExpSystem.GetTotalExp(level + 1, expType) - totalExp);    // 距離升級所需EXP
    public int maxLevel => GetMaxLevel();  // 最高等級 
    public uint maxExp => (PetExpSystem.GetTotalExp(maxLevel, expType));

    public PetExp() {}

    public PetExp(int _id, int _level, uint exp = 0) {
        id = _id;
        level = _level;
        totalExp = (exp == 0) ? PetExpSystem.GetTotalExp(_level, expType) : exp;
    }

    public PetExp(PetExp rhs) {
        id = rhs.id;
        level = rhs.level;
        fixedMaxLevel = rhs.fixedMaxLevel;
        totalExp = rhs.totalExp;
    }

    public int GetMaxLevel()
    {
        if (fixedMaxLevel > 0)
            return fixedMaxLevel;

        if (id.IsInRange(20001, 30000))
            return 105;

        return 100;
    }

    /// <summary>
    /// Let this pet gain exp. Return whether it should evolve.
    /// </summary>
    /// <param name="exp">Gain how much exp</param>
    /// <returns>evolve or not</returns>
    public bool GainExp(uint exp) {
        if (level >= maxLevel)
            return false;

        if (exp < levelUpExp) {
            totalExp += exp;
            return false;
        }

        while ((exp >= levelUpExp) && (level < maxLevel)) {
            exp -= levelUpExp;
            totalExp += levelUpExp;
            level++;
        }

        totalExp += exp;

        return (level >= evolveLevel) && (info.evolvePetId != 0) && (info.evolveLevel != 0);
    }

    public void LevelDown(int toWhichLevel) {
        if (!toWhichLevel.IsInRange(1, level))
            return;

        totalExp = PetExpSystem.GetTotalExp(toWhichLevel, expType);
        level = toWhichLevel;
        return;
    }

}
