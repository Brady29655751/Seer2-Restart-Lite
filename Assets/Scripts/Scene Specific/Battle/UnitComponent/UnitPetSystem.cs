using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitPetSystem 
{
    public BattlePet[] petBag = new BattlePet[6];
    public int cursor = 0;
    public int chain = 0;
    public BattlePet pet => petBag[cursor];
    public int petNum => petBag.Count(x => x != null);
    public int alivePetNum => petBag.Count(x => (x != null) && (!x.isDead));
    public int deadPetNum => petBag.Count(x => (x != null) && (x.isDead));

    public UnitPetSystem(BattlePet[] originalPetBag) {
        petBag = new BattlePet[originalPetBag.Length];
        Array.Copy(originalPetBag, petBag, originalPetBag.Length);
        cursor = 0;
        chain = 0;
    }

    public UnitPetSystem(UnitPetSystem rhs) {
        petBag = rhs.petBag.Select(x => (x == null) ? null : new BattlePet(x)).ToArray();
        cursor = rhs.cursor;
        chain = rhs.chain;
    }

    public void OnTurnStart(Unit thisUnit, BattleState state) {
        chain = 0;
        pet.stayTurn += 1;
        pet.OnTurnStart(thisUnit, state);
    }

    public void RefreshStayTurn() {
        foreach (var p in petBag) {
            if (p != null)
                p.stayTurn = 0;
        }
    }

    public float GetPetSystemIdentifier(string id) {
        return id switch {
            "cursor" => cursor,
            "chain" => chain,
            "aliveNum" => alivePetNum,
            "deadNum" => deadPetNum,
            _ => float.MinValue,
        };
    }

    public bool TryGetPetSystemIdentifier(string id, out float num) {
        num = GetPetSystemIdentifier(id);
        return num != float.MinValue;
    }

}
