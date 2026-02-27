using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PetHitInfo
{
    public const int DATA_COL = 7;
    public static int[] dataIndexes => new int[] { 2, 4, 3, 5, 6 };

    public int skinId;
    public string name;
    public Dictionary<PetAnimationType, short> frameDict = new Dictionary<PetAnimationType, short>();
    public Dictionary<PetAnimationType, short> videoFrameDict = new Dictionary<PetAnimationType, short>();
    public short physics => GetFrameByType(PetAnimationType.Physic);
    public short attribute => GetFrameByType(PetAnimationType.Property);
    public short special => GetFrameByType(PetAnimationType.Special);
    public short critical => GetFrameByType(PetAnimationType.Super);
    public short fit => GetFrameByType(PetAnimationType.SecondSuper);

    public PetHitInfo()
    {
    }

    public PetHitInfo(string[] _data, int startIndex = 0)
    {
        string[] _slicedData = new string[DATA_COL];
        Array.Copy(_data, startIndex, _slicedData, 0, _slicedData.Length);
        skinId = int.Parse(_slicedData[0]);
        name = _slicedData[1];

        for (PetAnimationType type = PetAnimationType.Physic; type <= PetAnimationType.SecondSuper; type++)
        {
            var dataIndex = dataIndexes.Get(((int)type) - 9);
            var frameData = _slicedData.Get(dataIndex).ToShortList('/');
            frameDict.Set(type, frameData.Get(0));
            videoFrameDict.Set(type, frameData.Get(1, (short)-1));
        }
    }

    public PetHitInfo(int skinId, string name, short physics, short attribute, short special, short critical, short fit)
    {
        this.skinId = skinId;
        this.name = name;
        this.frameDict = new Dictionary<PetAnimationType, short>()
        {
            {PetAnimationType.Physic, physics},
            {PetAnimationType.Property, attribute},
            {PetAnimationType.Special, special},
            {PetAnimationType.Super, critical},
            {PetAnimationType.SecondSuper, fit}
        };
    }

    public short GetFrameByType(PetAnimationType type)
    {
        return frameDict.Get(type);
    }

    public short GetVideoStartFrameByType(PetAnimationType type)
    {
        return videoFrameDict.Get(type, (short)-1);
    }

    public string GetVideoUrl(PetAnimationType type)
    {
        var prefix = PetInfo.IsMod(skinId) ? $"Mod/Pets/anim/video" : $"PetAnimation/pet/video";
        return $"{Application.persistentDataPath}/{prefix}/{skinId}-{type}.mp4";
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