using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LearnSkillInfo {
    public Skill skill;
    public SecretType secretType;
    public int value;
    public string learnDescription => GetLearnDescription();

    public LearnSkillInfo() {}

    public LearnSkillInfo(Skill skill, int learnLevel) {
        this.skill = (skill == null) ? null : new Skill(skill);
        this.secretType = (SecretType)(learnLevel / 10000);
        this.value = learnLevel % 10000;
    }

    public bool Condition(Pet pet, BattleState endState) {
        Func<Pet, BattleState, bool> GetCondition = secretType switch {
            SecretType.WinFightNum => this.WinFightNum,
            SecretType.LoseFightNum => this.LoseFightNum,
            SecretType.MeHaveBuff => this.MeHaveBuff,
            SecretType.OpHaveBuff => this.OpHaveBuff,
            SecretType.LessThanHp => this.LessThanHp,
            SecretType.EqualTurnNum => this.EqualTurnNum,
            SecretType.AtWeather => this.AtWeather,
            SecretType.UseSkill => this.UseSkill,
            SecretType.GreaterThanDamage => this.GreaterThanDamage,
            SecretType.SpecialPet => this.SpecialPet,
            SecretType.SpecailTrain => ((p, s) => false),
            _ => this.GreaterThanLevel,
        };
        return GetCondition(pet, endState);
    }

    public string GetLearnDescription() {
        return secretType switch {
            SecretType.WinFightNum => this.WinFightNum(),
            SecretType.LoseFightNum => this.LoseFightNum(),
            SecretType.MeHaveBuff => this.MeHaveBuff(),
            SecretType.OpHaveBuff => this.OpHaveBuff(),
            SecretType.LessThanHp => this.LessThanHp(),
            SecretType.EqualTurnNum => this.EqualTurnNum(),
            SecretType.AtWeather => this.AtWeather(),
            SecretType.UseSkill => this.UseSkill(),
            SecretType.GreaterThanDamage => this.GreaterThanDamage(),
            SecretType.SpecialPet => this.SpecialPet(),
            _ => this.GreaterThanLevel(),
        };
    }
}

public enum SecretType {
    GreaterThanLevel = 0,
    WinFightNum = 1,
    LoseFightNum = 2,
    MeHaveBuff = 3,
    OpHaveBuff = 4,
    LessThanHp = 5,
    EqualTurnNum = 6,
    AtWeather = 7,
    UseSkill = 8,
    GreaterThanDamage = 9,
    SpecialPet = 10,
    SpecailTrain = 11,
}
