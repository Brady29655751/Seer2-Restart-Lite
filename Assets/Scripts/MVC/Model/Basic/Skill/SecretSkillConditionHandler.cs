using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SecretSkillConditionHandler
{
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
        return endState.myUnit.skillSystem.skillDamage >= secretSkillInfo.value;
    }

    public static bool SpecialPet(this LearnSkillInfo secretSkillInfo, Pet pet, BattleState endState) {
        List<int> petId = secretSkillInfo.value switch {
            1 => new List<int>(){ 3, 6, 9 },
            2 => new List<int>(){ 15 },
            _ => new List<int>(),    
        };
        return petId.Select(x => endState.myUnit.petSystem.petBag.Any(y => (y != null) && (y.id == x))).All(x => x);
    }

}
