using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitPetSystem 
{
    public BattlePet[] petBag = new BattlePet[6];
    public int cursor = 0;
    public BattlePet pet => petBag[cursor];
    public int petNum => petBag.Count(x => x != null);
    public int alivePetNum => petBag.Count(x => (x != null) && (!x.isDead));
    public int deadPetNum => petBag.Count(x => (x != null) && (x.isDead));

    public UnitPetSystem(BattlePet[] originalPetBag) {
        petBag = new BattlePet[originalPetBag.Length];
        Array.Copy(originalPetBag, petBag, originalPetBag.Length);
        cursor = 0;
    }

    public UnitPetSystem(UnitPetSystem rhs) {
        petBag = rhs.petBag.Select(x => (x == null) ? null : new BattlePet(x)).ToArray();
        cursor = rhs.cursor;
    }

    public void OnTurnStart(Unit thisUnit, BattleState state) {
        pet.OnTurnStart(thisUnit, state);
    }

}
