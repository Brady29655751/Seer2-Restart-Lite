using System.Xml.Serialization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PetTalent
{
    [XmlAttribute] public int id;
    public int iv;  // 個體值(資質), from 0 to 31
    public Status ev = new Status();  // 學習力, from 0 to 255
    public int evStorage = 0;   // 剩餘學習力

    public PetTalentInfo info => Database.instance.GetPetInfo(id)?.talent;
    public int IVRankId => (int)IVRank;
    public IVRanking IVRank => PetIVSystem.GetIVRank(iv);     // 資質評級：一般、良、優、稀、極

    public PetTalent() {}

    public PetTalent(int _id) {
        id = _id;
        iv = Random.Range(0, 32);
        ev = new Status();
        evStorage = 0;
    }

    public PetTalent(int _id, PetTalent rhs) {
        id = _id;
        iv = rhs.iv;
        ev = new Status(rhs.ev);
        evStorage = rhs.evStorage;
    }

    public PetTalent(PetTalent rhs) {
        id = rhs.id;
        iv = rhs.iv;
        ev = new Status(rhs.ev);
        evStorage = rhs.evStorage;
    }

    public void SetEV(Status newEV) {
        ev = new Status(newEV);
    }

    public void SetEV(int type, int num) {
        ev[type] = Mathf.Clamp(num, 0, 255);
    }

    public void SetEVStorage(int num) {
        evStorage = Mathf.Clamp(num, 0, (int)(510 - ev.sum));
    }

    public void AddEVStorage(int num) {
        SetEVStorage(evStorage + num);
    }

    public void ResetEV() {
        evStorage += (int)ev.sum;
        ev = new Status();
    }


}

