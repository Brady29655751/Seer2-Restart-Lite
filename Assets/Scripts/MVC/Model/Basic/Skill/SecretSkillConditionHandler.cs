using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SecretSkillConditionHandler
{
    public static List<List<int>> specialPetIdList = new List<List<int>>() 
    {
        new List<int>(),
        new List<int>(){ 3, 6, 9 },
        new List<int>(){ 15 },
        new List<int>(){ 100, 810 },
        new List<int>(){ 95, 96 },
        new List<int>(){ 92, 93, 94 },
        new List<int>(){ 175, 177 },
        new List<int>(){ 157, 248 },
        new List<int>(){ 97, 98, 99 },
        new List<int>(){ 990, 1990 },
        new List<int>(){ 101, 102 },
        new List<int>(){ 103, 104 },
        new List<int>(){ 10070, 10261, 10798, 10875, 11633 },
    };

    public static bool GreaterThanLevel(this LearnSkillInfo secretSkillInfo, Pet pet, BattleState endState) {
        return pet.level >= secretSkillInfo.value;
    }

    public static bool WinFightNum(this LearnSkillInfo secretSkillInfo, Pet pet, BattleState endState) {
        return pet.record.winFightNum >= secretSkillInfo.value;
    }

    public static bool LoseFightNum(this LearnSkillInfo secretSkillInfo, Pet pet, BattleState endState) {
        return pet.record.loseFightNum >= secretSkillInfo.value;
    }

    public static bool MeHaveBuff(this LearnSkillInfo secretSkillInfo, Pet pet, BattleState endState) {
        return endState.myUnit.pet.buffController.GetBuff(secretSkillInfo.value) != null;
    }

    public static bool OpHaveBuff(this LearnSkillInfo secretSkillInfo, Pet pet, BattleState endState) {
        return endState.opUnit.pet.buffController.GetBuff(secretSkillInfo.value) != null;
    }

    public static bool LessThanHp(this LearnSkillInfo secretSkillInfo, Pet pet, BattleState endState) {
        return endState.myUnit.pet.hp <= (endState.myUnit.pet.maxHp * 1f / secretSkillInfo.value);
    }

    public static bool EqualTurnNum(this LearnSkillInfo secretSkillInfo, Pet pet, BattleState endState) {
        return endState.turn == secretSkillInfo.value;
    }

    public static bool AtWeather(this LearnSkillInfo secretSkillInfo, Pet pet, BattleState endState) {
        return endState.weather == secretSkillInfo.value;
    }

    public static bool UseSkill(this LearnSkillInfo secretSkillInfo, Pet pet, BattleState endState) {
        return endState.myUnit.skill.id == (10000 + secretSkillInfo.value);
    }

    public static bool GreaterThanDamage(this LearnSkillInfo secretSkillInfo, Pet pet, BattleState endState) {
        return endState.myUnit.skillSystem.totalSkillDamage >= secretSkillInfo.value;
    }

    public static bool SpecialPet(this LearnSkillInfo secretSkillInfo, Pet pet, BattleState endState) {
        List<int> petId = specialPetIdList.Get(secretSkillInfo.value, new List<int>());
        return petId.Select(x => endState.myUnit.petSystem.petBag.Any(y => (y != null) && (y.id == x))).All(x => x);
    }

}
