using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using Photon.Pun;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using ExitGames.Client.Photon.StructWrapping;

public class Battle
{
    public BattleManager UI { get => BattleManager.instance; }
    public BattleInfo info = null;
    public BattleSettings settings => currentState?.settings;
    public BattleResult result => currentState?.result;
    public BattleState lastState, currentState;
    public BattlePhase currentPhase;
    
    public int autoSkillCursor = 0;
    public List<int> autoSkillOrder = new List<int>();
    public bool isAutoSuperSkill = false;

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
        IEnumerable<BattlePet> myPetBag = null;
        if (info.settings.mode == BattleMode.YiTeRogue)
            myPetBag = YiTeRogueData.instance.battlePetBag;
        else if (ListHelper.IsNullOrEmpty(player))
            myPetBag = petBag.Take(info.settings.petCount).Select(x => BattlePet.GetBattlePet(x));
        else
            myPetBag = player.Take(info.settings.petCount).Select(x => BattlePet.GetBattlePet(x));

        BattlePet[] opPetBag = enemy.Select(x => BattlePet.GetBattlePet(x)).ToArray();

        Init(myPetBag.ToArray(), opPetBag, info.settings);
    }

    /// <summary>
    /// Create battle from Photon Network Hashtable for PVP.
    /// </summary>
    /// <param name="roomHash">Room properties</param>
    /// <param name="myHash">Local Player properties</param>
    /// <param name="opHash">Opponent properties</param>
    public Battle(Hashtable roomHash, Hashtable myHash, Hashtable opHash) {
        var seed = (int)roomHash["seed"];
        var petCount = (int)roomHash["count"];
        var buffList = (int[])roomHash["buff"];
        var iv = buffList.Contains(Buff.BUFFID_PVP_IV_120) ? 120 : 31;

        var roomSettings = new BattleSettings() {
            seed = seed,
            mode = BattleMode.PVP,
            petCount = petCount,
            parallelCount = (petCount == 2) ? 2 : 1,
            time = (int)roomHash["time"],
            initBuffs = buffList.Select((x, i) => new KeyValuePair<string, Buff>("rule[" + i + "]", new Buff(x))).ToList(),
            weather = 0,
            isSimulate = true,
            isEscapeOK = true,
            isItemOK = false,
            isCaptureOK = false,
        };
            
        var myPetBag = BattlePet.GetBattlePetBag(myHash, roomSettings.petCount, iv);
        var opPetBag = BattlePet.GetBattlePetBag(opHash, roomSettings.petCount, iv);
        var masterPetBag = PhotonNetwork.IsMasterClient ? myPetBag : opPetBag;
        var clientPetBag = PhotonNetwork.IsMasterClient ? opPetBag : myPetBag;
        Init(masterPetBag, clientPetBag, roomSettings);
        // RecordBattle(masterPetBag.Select(x => (x == null) ? null : new Pet(x)).ToArray(), 
        //     clientPetBag.Select(x => (x == null) ? null : new Pet(x)).ToArray(), roomSettings, PhotonNetwork.IsMasterClient);
    }

    private void Init(BattlePet[] masterPetBag, BattlePet[] clientPetBag, BattleSettings settings) {
        Player.instance.currentBattle = this;
        Random.InitState(settings.seed);

        // 精靈大亂鬥模式
        if (settings.initBuffs.Exists(x => (x.Value != null) && (x.Value.id == Buff.BUFFID_PET_EXCHANGE)))
        {
            var allPetBag = masterPetBag.Concat(clientPetBag).Where(x => x != null).ToList();
            var randomPetBag = allPetBag.Random(allPetBag.Count, false);

            masterPetBag = randomPetBag.Take(allPetBag.Count / 2).ToArray();
            clientPetBag = randomPetBag.Skip(allPetBag.Count / 2).ToArray();
        }

        if (!settings.isPVP)
        {
            // 大亂鬥補丁：PVE 時玩家方要還原為正常精靈
            masterPetBag.ForEach((x, i) =>
            {
                if (x == null)
                    return;

                x.skillController.loopSkills = null;
                x.buffController.buffs.RemoveAll(buff => buff.id == -3);
            });

            // 尼爾補丁
            if (settings.isCaptureOK && (Player.instance.currentMap.worldId == 1))
            {
                if (NpcConditionHandler.GetRandom("<", "0~256", "1"))
                {
                    clientPetBag[0] = BattlePet.GetBattlePet(new Pet(10077, clientPetBag[0]?.level ?? 1));
                }
            }
        }

        this.lastState = null;
        Unit masterTurn = new Unit(masterPetBag, 1, settings);
        Unit clientTurn = new Unit(clientPetBag, -1, settings);
        this.currentState = new BattleState(settings, masterTurn, clientTurn);
        this.currentPhase = new BattleStartPhase();
    }

    private void RecordBattle(Pet[] masterPetBag, Pet[] clientPetBag, BattleSettings settings, bool isMaster) {
        BattleRecord record = new BattleRecord() {
            isMaster = isMaster,
            settings = new BattleSettings(settings){ mode = BattleMode.Record },
            masterPetBag = masterPetBag,
            clientPetBag = clientPetBag,
            date = DateTime.Now,
        };
        Player.instance.gameData.battleRecordStorage.Add(record);
        if (Player.instance.gameData.battleRecordStorage.Count > BattleRecord.MAX_RECORD_STORAGE_NUM)
            Player.instance.gameData.battleRecordStorage.RemoveAt(0);

        SaveSystem.SaveData();        
    }

    private void NextPhase() {
        while (currentPhase != null) {
            currentPhase.DoWork();
            if (currentPhase == null)
                break;
                
            lastState = new BattleState(currentState);
            currentState = currentPhase.state;   
            currentPhase = currentPhase.GetNextPhase();
        }
    }

    public void OnBattleStart() {
        NextPhase();
    }

    /// <returns>whether all units are ready or not</returns>
    public bool SetSkill(Skill skill, bool isMe) {
        if (currentState == null)
            return false;

        Unit myUnit = currentState.myUnit;
        Unit opUnit = currentState.opUnit;
        Unit unit = (isMe ? myUnit : opUnit);
        Unit rhsUnit = (isMe ? opUnit : myUnit);

        if (settings.parallelCount > 1) {
            var sourceIndex = int.Parse(skill.options.Get("parallel_source_index", "0"));
            unit.parallelSkillSystems[sourceIndex].skill = skill;
        } else
            unit.SetSkill(skill);

        if (isMe) {
            if (!skill.IsSelectReady()) {
                UI.SetSkillSelectMode(true);
                UI.SelectOption(1);
                return false;
            }

            if (settings.parallelCount > 1) {
                if (!unit.isReady) {
                    UI.PVPSetSkillToOthers(skill);

                    var nextCursor = unit.petSystem.GetNextCursorCircular();
                    if ((skill.type != SkillType.逃跑) && (nextCursor != unit.petSystem.cursor)) {
                        unit.petSystem.cursor = nextCursor;
                        UI.SetState(null, currentState);
                        UI.ProcessQuery(true);
                    }
                    return false;
                }
            }

            UI.SetSkillSelectMode(false);
            UI.SetBottomBarInteractable(false);
            UI.PVPSetSkillToOthers(skill);
        } else if (settings.mode == BattleMode.PVP)
            Player.instance.gameData.battleRecordStorage?.LastOrDefault()?.AddAction(skill.ToRPCData(settings), false);

        if ((settings.parallelCount <= 1) && (currentState.isAnyPetDead)) {
            unit.SetSkill(null);
            currentPhase = currentPhase ?? new PassivePetChangePhase();
            ((PassivePetChangePhase)currentPhase).SetSkill(skill, isMe);

            currentPhase = currentPhase.GetNextPhase();
            if (currentPhase.phase != EffectTiming.OnPassivePetChange)
                NextPhase();

            return false;
        }

        if ((settings.mode != BattleMode.PVP) && (settings.mode != BattleMode.Record) && (opUnit.skill == null)) {
            var parallelCount = Mathf.Min(settings.parallelCount, opUnit.petSystem.alivePetNum);
            for (int i = 0; i < parallelCount; i++) {
                var cursor = opUnit.petSystem.cursor;
                var nextCursor = opUnit.petSystem.GetNextCursorCircular();
                var defaultSkill = opUnit.pet.GetDefaultSkill();
                if (opUnit.pet.isDead)
                    defaultSkill = (settings.parallelCount > 1) ? null : Skill.GetPetChangeSkill(cursor, nextCursor, true);   

                if (parallelCount > 1)
                    defaultSkill.SetParallelIndex(cursor, myUnit.petSystem.petBag.FindIndex(x => (x != null) && (!x.isDead)));
                
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
            return true;
        }

        return false;
    }

    public virtual float GetBattleIdentifier(string id) {
        return currentState.GetStateIdentifier(id);
    }

}
