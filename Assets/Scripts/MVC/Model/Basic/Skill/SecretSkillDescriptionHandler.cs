using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SecretSkillDescriptionHandler
{
    public static string GreaterThanLevel(this LearnSkillInfo secretSkillInfo) {
        return "等级达到" + secretSkillInfo.value + "级";
    }

    public static string WinFightNum(this LearnSkillInfo secretSkillInfo) {
        return "获得" + secretSkillInfo.value + "场对战胜利";
    }

    public static string LoseFightNum(this LearnSkillInfo secretSkillInfo) {
        return "累计" + secretSkillInfo.value + "场对战失败后首次击败对手";
    }

    public static string MeHaveBuff(this LearnSkillInfo secretSkillInfo) {
        return "自身处于" + Buff.GetBuffInfo(secretSkillInfo.value).name + "状态时击败对手";
    }

    public static string OpHaveBuff(this LearnSkillInfo secretSkillInfo) {
        return "对手处于" + Buff.GetBuffInfo(secretSkillInfo.value).name + "状态时击败对手";
    }

    public static string LessThanHp(this LearnSkillInfo secretSkillInfo) {
        return "自身体力低于1/" + secretSkillInfo.value + "时击败对手";
    }

    public static string EqualTurnNum(this LearnSkillInfo secretSkillInfo) {
        return "在第" + secretSkillInfo.value + "回合击败对手";
    }

    public static string AtWeather(this LearnSkillInfo secretSkillInfo) {
        return Buff.GetWeatherBuff(secretSkillInfo.value).name + "环境下击败对手";
    }

    public static string UseSkill(this LearnSkillInfo secretSkillInfo) {
        return "使用" + Skill.GetSkill(10000 + secretSkillInfo.value).name + "击败对手";
    }

    public static string GreaterThanDamage(this LearnSkillInfo secretSkillInfo) {
        return "最后一击的伤害超过" + secretSkillInfo.value;
    }

    public static string SpecialPet(this LearnSkillInfo secretSkillInfo) {
        string pet = secretSkillInfo.value switch {
            1 => "迪兰特、休罗斯、拉奥叶",
            2 => "笑笑葵",
            _ => string.Empty,    
        };
        return "背包中携带" + pet + "并获得胜利";
    }
}
