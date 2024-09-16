using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml.Serialization;
using JetBrains.Annotations;
using UnityEngine;

public class PetUI
{
    [XmlIgnore] public PetUIInfo info => Database.instance.GetPetInfo(id)?.ui;
    [XmlIgnore] public PetHitInfo hitInfo => Database.instance.GetPetHitInfo(skinId);
    [XmlIgnore] public PetSoundInfo soundInfo => Database.instance.GetPetSoundInfo(skinId);

    [XmlAttribute] public int id;
    [XmlAttribute] public int baseId;

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

    [XmlIgnore] public Sprite icon => PetUISystem.GetPetIcon(skinId);
    [XmlIgnore] public Sprite emblemIcon => PetUISystem.GetEmblemIcon(skinBaseId);
    [XmlIgnore] public Sprite battleImage => PetUISystem.GetPetBattleImage(skinId);
    [XmlIgnore] public Sprite idleImage => PetUISystem.GetPetIdleImage(skinId);


    public GameObject GetBattleAnim(PetAnimationType type)
    {
        return PetUISystem.GetPetAnimInstance(skinId, type);
    }

    public PetUI()
    {
    }

    public PetUI(int id, int baseId)
    {
        this.id = id;
        this.baseId = baseId;
    }

    public PetUI(PetUI rhs)
    {
        id = rhs.id;
        baseId = rhs.baseId;
    }
}