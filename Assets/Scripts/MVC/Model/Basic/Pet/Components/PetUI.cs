using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml.Serialization;
using UnityEngine;

public class PetUI
{
    [XmlIgnore] public PetUIInfo info => Database.instance.GetPetInfo(id)?.ui;
    [XmlIgnore] public PetUIInfo skinInfo => Database.instance.GetPetInfo(skinId)?.ui;
    [XmlIgnore] public PetHitInfo hitInfo => Database.instance.GetPetHitInfo(skinInfo?.defaultAnimId ?? 0);
    [XmlIgnore] public PetSoundInfo soundInfo => Database.instance.GetPetSoundInfo(skinInfo?.defaultAnimId ?? 0);

    [XmlAttribute] public int id;
    [XmlAttribute] public int baseId;
    public List<int> specialSkinList;

    [XmlIgnore]
    public int skinId
    {
        get => (id == 0) ? info.defaultSkinId : id;
        set => id = value;
    }

    [XmlIgnore]
    public int skinBaseId
    {
        get => (baseId == 0) ? info.baseId : baseId;
        set => baseId = value;
    }

    [XmlIgnore] 
    public int animId => skinInfo?.defaultAnimId ?? 0;

    [XmlIgnore]
    public int hashId {
        get 
        {
            unchecked 
            {
                int hash = 17;
                hash = hash * 31 + skinId;
                return hash;
            }
        }
    }

    [XmlIgnore] public Sprite icon => PetUISystem.GetPetIcon(skinId);
    [XmlIgnore] public Sprite emblemIcon => PetUISystem.GetEmblemIcon(skinBaseId);
    [XmlIgnore] public Sprite battleImage => PetUISystem.GetPetBattleImage(skinId);
    [XmlIgnore] public Sprite idleImage => PetUISystem.GetPetIdleImage(skinId);


    public GameObject GetBattleAnim(PetAnimationType type)
    {
        return PetUISystem.GetPetAnimInstance(animId, type);
    }

    public void GetBattleAnimAsync(PetAnimationType type, Action<GameObject> onSuccess, Action<float> onProgress = null)
    {
        PetUISystem.GetPetAnimInstanceAsync(animId, type, onSuccess, onProgress);
    }

    public void PreloadPetAnimAsync(Action onSuccess = null, Action<float> onProgress = null) 
    {   
        PetUISystem.PreloadPetAnimAsync(animId, onSuccess, onProgress);
    }

    public PetUI()
    {
    }

    public PetUI(int id, int baseId, List<int> specialSkinList = null)
    {
        this.id = id;
        this.baseId = baseId;
        this.specialSkinList = specialSkinList ?? new List<int>();
    }

    public PetUI(PetUI rhs)
    {
        id = rhs.id;
        baseId = rhs.baseId;
        specialSkinList = rhs.specialSkinList;
    }
}