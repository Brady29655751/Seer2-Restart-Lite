using System;

public class PetHitInfo
{
    public const int DATA_COL = 7;

    public int skinId;
    public string name;
    public short physics;
    public short attribute;
    public short special;
    public short critical;
    public short fit;

    public PetHitInfo()
    {
    }

    public PetHitInfo(string[] _data, int startIndex = 0)
    {
        string[] _slicedData = new string[DATA_COL];
        Array.Copy(_data, startIndex, _slicedData, 0, _slicedData.Length);
        skinId = int.Parse(_slicedData[0]);
        name = _slicedData[1];
        physics = short.Parse(_slicedData[2]);
        attribute = short.Parse(_slicedData[3]);
        special = short.Parse(_slicedData[4]);
        critical = short.Parse(_slicedData[5]);
        fit = short.Parse(_slicedData[6]);
    }

    public PetHitInfo(int skinId, string name, short physics, short attribute, short special, short critical, short fit)
    {
        this.skinId = skinId;
        this.name = name;
        this.physics = physics;
        this.attribute = attribute;
        this.special = special;
        this.critical = critical;
        this.fit = fit;
    }

    public short GetFrameByType(PetAnimationType type)
    {
        return type switch
        {
            PetAnimationType.Physic => physics,
            PetAnimationType.Property => attribute,
            PetAnimationType.Special => special,
            PetAnimationType.Super => critical,
            PetAnimationType.SecondSuper => fit,
            PetAnimationType.JointSuper => fit,
            _ => 0
        };
    }

    public string[] GetRawInfoStringArray()
    {
        return new string[]
        {
            skinId.ToString(), name, physics.ToString(), attribute.ToString(), special.ToString(), critical.ToString(),
            fit.ToString()
        };
    }
}