using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class Battle
{
    public BattleManager UI { get => BattleManager.instance; }
    public BattleInfo info = null;
    public BattleSettings settings => currentState?.settings;
    public BattleResult result => currentState?.result;
    public BattleState lastState, currentState;
    public BattlePhase currentPhase;

    public Battle(Pet[] myPetBag, Pet[] opPetBag, BattleSettings settings) {
        BattlePet[] player = myPetBag.Take(settings.petCount).Select(x => BattlePet.GetBattlePet(x)).ToArray();
        BattlePet[] enemy = opPetBag.Select(x => BattlePet.GetBattlePet(x)).ToArray();

        Init(player, enemy, settings);
    }

    public Battle(BattlePet[] player, BattlePet[] enemy, BattleSettings settings) {
        Init(player.Take(settings.petCount).ToArray(), enemy, settings);
    }

    /// <summary>
    /// Create battle from XML-serialized battle info.
    /// </summary>
    public Battle(BattleInfo info) {
        this.info = info;

        List<BossInfo> player = info.playerInfo;
        List<BossInfo> enemy = info.enemyInfo;

        Pet[] petBag = Player.instance.petBag;
        BattlePet[] myPetBag = (((player == null) || (player.Count == 0)) ?
            petBag.Take(info.settings.petCount).Select(x => BattlePet.GetBattlePet(x)) :
            player.Take(info.settings.petCount).Select(x => BattlePet.GetBattlePet(x))).ToArray();
        BattlePet[] opPetBag = enemy.Select(x => BattlePet.GetBattlePet(x)).ToArray();
        
        Init(myPetBag, opPetBag, info.settings);
    }

    /// <summary>
    /// Create battle from Photon Network Hashtable for PVP.
    /// </summary>
    /// <param name="roomHash">Room properties</param>
    /// <param name="myHash">Local Player properties</param>
    /// <param name="opHash">Opponent properties</param>
    public Battle(Hashtable roomHash, Hashtable myHash, Hashtable opHash) {
        Random.InitState((int)roomHash["seed"]);

        var petCount = (int)roomHash["count"];
        var roomSettings = new BattleSettings() {
            mode = BattleMode.PVP,
            petCount = petCount,
            parallelCount = (petCount == 2) ? 2 : 1,
            time = (int)roomHash["time"],
            initBuffs = ((int[])roomHash["buff"]).Select((x, i) => new KeyValuePair<string, Buff>("rule[" + i + "]", new Buff(x))).ToList(),
            weather = 0,
            isSimulate = true,
            isEscapeOK = true,
            isItemOK = false,
            isCaptureOK = false,
        };
            
        var myPetBag = BattlePet.GetBattlePetBag(myHash, roomSettings.petCount);
        var opPetBag = BattlePet.GetBattlePetBag(opHash, roomSettings.petCount);
        if (PhotonNetwork.IsMasterClient) {
            Init(myPetBag, opPetBag, roomSettings);
        } else {
            Init(opPetBag, myPetBag, roomSettings);
        }
    }

    private void Init(BattlePet[] masterPetBag, BattlePet[] clientPetBag, BattleSettings settings) {
        Player.instance.currentBattle = this;

        this.lastState = null;
        Unit masterTurn = new Unit(masterPetBag, 1, settings);
        Unit clientTurn = new Unit(clientPetBag, -1, settings);
        this.currentState = new BattleState(settings, masterTurn, clientTurn);
        this.currentPhase = new BattleStartPhase();
    }

    private void NextPhase() {
        while (currentPhase != null) {
            currentPhase.DoWork();
            lastState = new BattleState(currentState);
            currentState = currentPhase.state;   
            currentPhase = currentPhase.GetNextPhase();
        }
    }

    public void OnBattleStart() {
        NextPhase();
    }

    public void SetSkill(Skill skill, bool isMe) {
        if (currentState == null)
            return;

        Unit myUnit = currentState.myUnit;
        Unit opUnit = currentState.opUnit;
        Unit unit = (isMe ? myUnit : opUnit);
        Unit rhsUnit = (isMe ? opUnit : myUnit);

        unit.SetSkill(skill);

        if (isMe) {
            if (!skill.IsSelectReady()) {
                UI.SetSkillSelectMode(true);
                UI.SelectOption(1);
                return;
            }

            if (settings.parallelCount > 1) {
                if (skill.type != SkillType.逃跑) {
                    //var state = new BattleState(currentState);
                    unit.petSystem.cursor = unit.petSystem.GetNextCursorCircular();
                    UI.SetState(null, currentState);
                    UI.ProcessQuery(true);
                }
                
                if (!unit.isReady)
                    return;
            }

            UI.SetSkillSelectMode(false);
            UI.SetBottomBarInteractable(false);
            UI.PVPSetSkillToOthers(skill);
        }

        if ((settings.parallelCount <= 1) && (currentState.isAnyPetDead)) {
            unit.SetSkill(null);
            currentPhase = currentPhase ?? new PassivePetChangePhase();
            ((PassivePetChangePhase)currentPhase).SetSkill(skill, isMe);

            currentPhase = currentPhase.GetNextPhase();
            if (currentPhase.phase != EffectTiming.OnPassivePetChange)
                NextPhase();

            return;
        }

        if ((settings.mode != BattleMode.PVP) && (opUnit.skill == null)) {
            var parallelCount = Mathf.Min(settings.parallelCount, opUnit.petSystem.alivePetNum);
            for (int i = 0; i < parallelCount; i++) {
                var cursor = opUnit.petSystem.cursor;
                var nextCursor = opUnit.petSystem.GetNextCursorCircular();
                var defaultSkill = opUnit.pet.GetDefaultSkill();
                if (opUnit.pet.isDead)
                    defaultSkill = (settings.parallelCount > 1) ? null : Skill.GetPetChangeSkill(cursor, nextCursor, true);   
                
                opUnit.SetSkill(defaultSkill);

                if (parallelCount > 1)
                    opUnit.petSystem.cursor = nextCursor;
            }
        }

        if (currentState.isAllUnitReady) {
            myUnit.parallelSkillSystems.ForEach(x => x.EnsureSkillNotNull());
            opUnit.parallelSkillSystems.ForEach(x => x.EnsureSkillNotNull());
            currentPhase = new TurnReadyPhase();
            NextPhase();
            return;
        }
    }

    public virtual float GetBattleIdentifier(string id) {
        return currentState.GetStateIdentifier(id);
    }

}
