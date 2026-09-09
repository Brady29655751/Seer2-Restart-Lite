using System;
using UnityEngine;
using UnityEngine.UI;

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

        var sign = PetInfo.IsMod(petId) ? 800000 : 200000;
        var buffInfo = Buff.GetBuffInfo(Mathf.Abs(petId) + sign);

        return buffInfo?.GetIcon();
    }

    public static Sprite GetPetBattleImage(int petId)
    {
        return ResourceManager.instance.GetLocalAddressables<Sprite>("Pets/battle/" + petId, PetInfo.IsMod(petId));
    }

    public static Sprite GetPetIdleImage(int petId)
    {
        return ResourceManager.instance.GetLocalAddressables<Sprite>("Pets/pet/" + petId, PetInfo.IsMod(petId));
    }

    public static PetTransformInfo GetPetTransformInfo(int petId, PetAnimationType type)
    {
        return Database.instance.GetPetTransformInfo(petId);
    }

    public static GameObject GetPetAnimInstance(int petId, PetAnimationType type)
    {
        string animName = type.ToString();
        var anim = ResourceManager.instance.GetPetAnimInstance(petId, $"{petId}-" + animName);
        var info = GetPetTransformInfo(petId, type);
        if ((anim != null) && (info != null))
        {
            anim.transform.position = info.GetPositionByType(type, anim.transform.position);
            anim.transform.localScale = info.GetScaleByType(type, anim.transform.localScale);
        }

        return anim;
    }

    public static void GetPetAnimInstanceAsync(int petId, PetAnimationType type, Action<GameObject> onSuccess, Action<float> onProgress = null)
    {
        string animName = type.ToString();
        ResourceManager.instance.GetPetAnimInstanceAsync(petId, $"{petId}-" + animName, (anim) => 
        {
            var info = GetPetTransformInfo(petId, type);
            if ((anim != null) && (info != null))
            {
                anim.transform.position = info.GetPositionByType(type, anim.transform.position);
                anim.transform.localScale = info.GetScaleByType(type, anim.transform.localScale);
            }
            onSuccess?.Invoke(anim);
        }, onProgress);
    }

    public static void PreloadPetAnimAsync(int petId, Action onSuccess = null, Action<float> onProgress = null)
    {
        ResourceManager.instance.GetPetAnimAssetBundleAsync(petId, assetBundle => onSuccess?.Invoke(), onProgress);
    }

    public static Sprite GetSprite(this StatusType statusType) {
        var statusName = Status.typeNames[(int)statusType];
        return ResourceManager.instance.Get<Sprite>("Panels/PetBag/Sprites/petValue/" + statusName);
    }

    public static Sprite GetSprite(this Element element)
    {
        int index = (int)element;
        return ResourceManager.instance.GetSprite("Elements/" + index.ToString()) ?? SpriteSet.Question;
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
    BeCriticalStruck = 14,
    Morph = 15,
    Present = 16,
}