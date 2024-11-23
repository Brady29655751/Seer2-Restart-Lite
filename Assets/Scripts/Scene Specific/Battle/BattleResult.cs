using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class BattleResult
{
    public BattleResultState state = BattleResultState.Fighting;
    public HashSet<int> fightPetCursors = new HashSet<int>() { 0 };
    public uint gainExpPerPet { get; private set; } = 0;
    public int gainEVStoragePerPet { get; private set; } = 0;
    public Pet capturePet = null;

    public bool isBattleEnd => (state != BattleResultState.Fighting);
    public bool isKO => ((state == BattleResultState.Win) || (state == BattleResultState.Lose));
    public bool isEscape => ((state == BattleResultState.MyEscape) || (state == BattleResultState.OpEscape));
    public bool isCapture => (state == BattleResultState.CaptureSuccess);
    public bool isMyWin => ((state == BattleResultState.Win) || (state == BattleResultState.OpEscape) || (state == BattleResultState.CaptureSuccess));
    public bool isOpWin => ((state == BattleResultState.Lose) || (state == BattleResultState.MyEscape));

    public BattleResult() {
        state = BattleResultState.Fighting;
        fightPetCursors = new HashSet<int>() { 0 };
    }

    public BattleResult(BattleResult rhs) {
        state = rhs.state;
        fightPetCursors = new HashSet<int>(rhs.fightPetCursors);
    }

    public float GetResultIdentifier(string id) {
        string trimId = id;
        if (id.TryTrimStart("capturePet.", out trimId))
            return (capturePet == null) ? -1 : capturePet.GetPetIdentifier(trimId);

        return id switch {
            "state" => (float)state,
            "fightPetCount" => fightPetCursors.Count,
            "isEnd" => isBattleEnd ? 1 : 0,
            "isKO" => isKO ? 1 : 0,
            "isEscape" => isEscape ? 1 : 0,
            "isCapture" => isCapture ? 1 : 0,
            "isMyWin" => isMyWin ? 1 : 0,
            "isOpWin" => isOpWin ? 1 : 0,
            _ => float.MinValue,
        };
    }

    public void SetState(BattleResultState state) {
        this.state = state;
    }

    public void AddFightPetCursor(int cursor) {
        fightPetCursors.Add(cursor);
    }

    public void ProcessResult(BattleState endState) {
        Random.InitState((int)DateTime.Now.Ticks);
        if (endState.settings.isSimulate)
            return;

        SetBasic(endState);
        SetRecord(endState);

        if (!isMyWin) {
            SaveSystem.SaveData();
            return;
        }

        if (state == BattleResultState.CaptureSuccess) {
            var pet = endState.opUnit.pet;
            capturePet = new Pet(pet.basic.baseId, endState.settings.captureLevel, pet.feature.hasEmblem);
            Pet.Add(capturePet);
            SaveSystem.SaveData();
            return;
        }
        
        SetTalent(endState);
        SetExp(endState);
        SetSkill(endState);
        
        SaveSystem.SaveData();
    }

    private void SetBasic(BattleState endState) {
        DoWork(endState, (state, battlePet, pet) => {
            pet.currentStatus.hp = Mathf.Min(pet.normalStatus.hp, battlePet.hp);
        });
    }

    private void SetRecord(BattleState endState) {
        DoWork(endState, (state, battlePet, pet) => {
            if (pet.level < 60)
                return;

            if (isOpWin) {
                pet.record.loseFightNum++;
                return;
            }
                
            if (isMyWin) {
                pet.record.winFightNum++;
                return;
            }
        });
    }

    private void SetTalent(BattleState endState) {
        gainEVStoragePerPet = endState.opUnit.petSystem.petNum;

        DoWork(endState, (state, battlePet, pet) => { 
            int battleEvMult = 1;

            if (battlePet.record.TryGetRecord("battleEvMult", out var battleEvMultExpr) &&
                battlePet.record.TryGetRecord("battleEvChance", out var battleEvChanceExpr) &&
                battlePet.record.TryGetRecord("battleEvBuff", out var battleEvBuffExpr)) {
                
                battleEvMult = int.Parse(battleEvMultExpr);

                int battleEvChance = int.Parse(battleEvChanceExpr) - 1;
                int battleEvBuffId = int.Parse(battleEvBuffExpr);

                if (battleEvChance <= 0) {
                    pet.feature.afterwardBuffIds.Remove(battleEvBuffId);
                    pet.record.SetRecord("battleEvChance", null);
                    pet.record.SetRecord("battleEvBuff", null);
                } else
                    pet.record.SetRecord("battleEvChance", battleEvChance);
            }

            pet.talent.AddEVStorage(gainEVStoragePerPet * battleEvMult); 
        });
    }

    private void SetExp(BattleState endState) {
        UnitPetSystem opUnitPet = endState.opUnit.petSystem;
        uint totalExp = (uint)opUnitPet.petBag.Where(x => x != null).Select(x => (x.exp.info.beatExpParam / 255f) * PetExpSystem.GetLevelExp(x.level, x.exp.expType)).Sum();
        gainExpPerPet = totalExp / (uint)(fightPetCursors.Count);

        DoWork(endState, (state, battlePet, pet) => { 
            uint battleExpMult = 1;

            if (battlePet.record.TryGetRecord("battleExpMult", out var battleExpMultExpr) &&
                battlePet.record.TryGetRecord("battleExpChance", out var battleExpChanceExpr) &&
                battlePet.record.TryGetRecord("battleExpBuff", out var battleExpBuffExpr)) {

                battleExpMult = uint.Parse(battleExpMultExpr);

                int battleExpChance = int.Parse(battleExpChanceExpr) - 1;
                int battleExpBuffId = int.Parse(battleExpBuffExpr);

                if (battleExpChance <= 0) {
                    pet.feature.afterwardBuffIds.Remove(battleExpBuffId);
                    pet.record.SetRecord("battleExpChance", null);
                    pet.record.SetRecord("battleExpBuff", null);
                } else
                    pet.record.SetRecord("battleExpChance", battleExpChance);
            }

            pet.GainExp(gainExpPerPet * battleExpMult); 
        });
    }

    private void SetSkill(BattleState endState) {
        var cursor = endState.myUnit.petSystem.cursor;
        Pet pet = Player.instance.petBag[cursor];
        
        if (pet.level < 60)
            return;

        var secretSkillInfo = pet.skills.secretSkillInfo.Where(x => !pet.skills.ownSkillId.Contains(x.skill.id)).ToArray();
        for (int i = 0; i < secretSkillInfo.Length; i++) {
            if (secretSkillInfo[i].Condition(pet, endState))
                pet.skills.LearnNewSkill(secretSkillInfo[i].skill);
        }
    }

    private void DoWork(BattleState endState, Action<BattleState, BattlePet, Pet> work) {
        foreach (var cursor in fightPetCursors) {
            Pet pet = Player.instance.petBag[cursor];
            BattlePet battlePet = endState.myUnit.petSystem.petBag[cursor];
            if (pet == null)
                continue;

            work?.Invoke(endState, battlePet, pet);
        }
    }

}

public enum BattleResultState {
    Error = -1,
    Fighting = 0,
    Win = 1,
    Lose = 2,
    MyEscape = 3,
    OpEscape = 4,
    CaptureSuccess = 5,
    Timeout = 99,
}
