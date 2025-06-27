using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitPetSystem 
{
    public BattlePet[] petBag = new BattlePet[6];
    public BattlePet[] backupPetBag = new BattlePet[6];
    public int cursor = 0;
    public int specialChance = 1;
    public BattlePet pet => petBag[cursor];
    public int petNum => petBag.Count(x => x != null);
    public int alivePetNum => petBag.Count(x => (x != null) && (!x.isDead));
    public int deadPetNum => petBag.Count(x => (x != null) && (x.isDead));

    public UnitPetSystem(BattlePet[] originalPetBag) {
        petBag = new BattlePet[originalPetBag.Length];
        backupPetBag = new BattlePet[originalPetBag.Length];
        Array.Copy(originalPetBag, petBag, originalPetBag.Length);
        cursor = 0;
        specialChance = 1;
    }

    public UnitPetSystem(UnitPetSystem rhs) {
        petBag = rhs.petBag.Select(x => (x == null) ? null : new BattlePet(x)).ToArray();
        backupPetBag = rhs.backupPetBag.Select(x => (x == null) ? null : new BattlePet(x)).ToArray();
        cursor = rhs.cursor;
        specialChance = rhs.specialChance;
    }

    public bool SwapBackupPet(int index)
    {
        if (!index.IsInRange(0, petBag.Length))
            return false;

        if (petBag[index] != null)
            petBag[index].stayTurn = 0;

        if (backupPetBag[index] != null)
            backupPetBag[index].stayTurn = 0;

        var pet = backupPetBag[index];
        backupPetBag[index] = petBag[index];
        petBag[index] = pet;

        return true;
    }

    public void OnTurnStart(Unit thisUnit, BattleState state)
    {
        var parallelCount = Mathf.Min(state.settings.parallelCount, alivePetNum);
        var parallelCursor = cursor;

        GetParallelPetBag(parallelCount).ForEach(x => x.OnTurnStart(thisUnit, state));

        if ((state.settings.parallelCount > 1) && (pet?.isDead ?? true))
            cursor = GetNextCursorCircular();
    }

    public void RefreshStayTurn() {
        foreach (var p in petBag) {
            if (p != null)
                p.stayTurn = 0;
        }
    }

    public List<BattlePet> GetParallelPetBag(int parallelCount) {
        var bag = new List<BattlePet>(){ pet };
        if (parallelCount == 1)
            return bag;
        
        int parallelCursor = cursor;
        while ((parallelCursor = GetNextCursorCircular(parallelCursor, (p) => true, (i) => i < parallelCount)) != cursor)
            bag.Add(petBag[parallelCursor]);

        return bag;
    }

    public List<int> GetParallelPetBagCursor(int parallelCount) {
        return GetParallelPetBag(parallelCount).Select(petBag.IndexOf).ToList();
    }

    /// <summary>
    /// Get next cursor that the pet is neither null (nor dead by default). <br/>
    /// If start is less than 0, use current cursor as start. <br/>
    /// If no pet meets condition, return start (return cursor if start &lt; 0).
    /// </summary>
    /// <param name="start">Start from which index. The index "start + 1" will be the first searched.</param>
    /// <param name="petFilter">pet filter</param>
    /// <param name="indexFilter">index filter</param>
    /// <returns></returns>
    public int GetNextCursorCircular(int start = -1, Func<BattlePet, bool> petFilter = null, Func<int, bool> indexFilter = null) {
        start = (start < 0) ? cursor : start;
        petFilter ??= (x) => !x.isDead;
        indexFilter ??= (x) => true;
        for (int i = 0; i < petBag.Length; i++) {
            var index = (start + i + 1) % petBag.Length;
            if (!indexFilter(index))
                continue;

            var pet = petBag[index];
            if ((pet != null) && petFilter(pet))
                return index;
        }
        return start;
    }

    public float GetPetSystemIdentifier(string id) {
        return id switch {
            "cursor" => cursor,
            "aliveNum" => alivePetNum,
            "deadNum" => deadPetNum,
            "specialChance" => specialChance,
            _ => float.MinValue,
        };
    }

    public bool TryGetPetSystemIdentifier(string id, out float num) {
        num = GetPetSystemIdentifier(id);
        return num != float.MinValue;
    }

}
