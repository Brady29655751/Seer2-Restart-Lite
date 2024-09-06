using System;
using UnityEngine;

public static class PetUISystem
{
    public static Sprite GetPetIcon(int petId)
    {
        return ResourceManager.instance.GetLocalAddressables<Sprite>("Pets/icon/" + petId, PetInfo.IsMod(petId));
    }

    public static Sprite GetEmblemIcon(int petId)
    {
        if (petId == 0)
            return ResourceManager.instance.GetSprite("Emblems/0");

        return ResourceManager.instance.GetLocalAddressables<Sprite>("Emblems/" + petId, PetInfo.IsMod(petId));
    }

    public static Sprite GetPetBattleImage(int petId)
    {
        return ResourceManager.instance.GetLocalAddressables<Sprite>("Pets/battle/" + petId, PetInfo.IsMod(petId));
    }

    public static GameObject GetPetAnimInstance(int petId, PetAnimationType type)
    {
        string animName = type switch
        {
            PetAnimationType.Idle => $"待机",
            PetAnimationType.Dying => $"濒死",
            PetAnimationType.Physic => $"物理攻击",
            PetAnimationType.Special => $"特殊攻击",
            PetAnimationType.Property => $"属性攻击",
            PetAnimationType.Super => $"必杀",
            PetAnimationType.SecondSuper => $"合体攻击", //其实不是,但暂定
            PetAnimationType.Win => $"胜利",
            PetAnimationType.Lose => $"失败",
            PetAnimationType.Hurt => $"被打",
            PetAnimationType.Evade => $"闪避",
            PetAnimationType.BeCriticalStruck => $"被暴击",
            PetAnimationType.JointSuper => $"合体攻击",
            PetAnimationType.Morph => ($"变身"),
            PetAnimationType.Present => ($"个性出场"),
            _ => throw new Exception("阿娘你个嗲列,没有这个动画的!!" + type)
        };
        return ResourceManager.instance.GetPetAnimInstance(petId, $"{petId}-" + animName);
    }

    public static Sprite GetSprite(this Element element)
    {
        int index = (int)element;
        return ResourceManager.instance.GetSprite("Elements/" + index.ToString());
    }

    public static Sprite GetSprite(this IVRanking ranking)
    {
        int index = (int)ranking;
        return ResourceManager.instance.GetSprite("IVRank/" + index.ToString());
    }
}

public enum PetAnimationType
{
    None = 0,
    Idle = 1,
    Dying = 2,
    Win = 3,
    Lose = 4,
    CaptureSuccess = 5,
    CaptureFail = 6,
    Hurt = 7,
    Evade = 8,
    Physic = 9,
    Special = 10,
    Property = 11,
    Super = 12,
    SecondSuper = 13,
    JointSuper = 14,
    BeCriticalStruck = 15,
    Morph = 16,
    Present = 17,
}