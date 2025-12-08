using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class BattleManager : Manager<BattleManager>
{
    public Battle battle => Player.instance.currentBattle;
    [SerializeField] private BattleSystemView systemView;
    [SerializeField] private BattleUnitView playerView, enemyView;
    [SerializeField] private List<BattleUnitView> doublePlayerView, doubleEnemyView;
    [SerializeField] private PhotonView masterView, clientView;

    private List<BattleUnitView> allUnitViews => GetAllUnitViews();
    private List<BattleUnitView> allPlayerViews => allUnitViews.Take(allUnitViews.Count / 2).ToList();
    private List<BattleUnitView> allEnemyViews => allUnitViews.TakeLast(allUnitViews.Count / 2).ToList();

    public bool isDone => allUnitViews?.All(x => x?.isDone ?? true) ?? true;
    public bool isSkillSelectMode { get; protected set; } = false;
    public BattleState currentState => (currentUIState == null) ? null : new BattleState(currentUIState);
    protected BattleState currentUIState = null;
    protected Queue<BattleState> queue = new Queue<BattleState>();

    // For battle record
    protected int recordActionIndex = 0;
    protected List<int> recordActionSteps = new List<int>();

    protected override void Awake()
    {
        base.Awake();
        if (PhotonNetwork.IsConnected)
        {
            var photonView = PhotonNetwork.IsMasterClient ? masterView : clientView;
            photonView.RequestOwnership();

            NetworkManager.instance.onDisconnectEvent += OnLocalPlayerDisconnect;
            NetworkManager.instance.onOtherPlayerLeftRoomEvent += OnOtherPlayerDisconnect;
        }
    }

    private void Start()
    {
        battle.OnBattleStart();
    }

    private List<BattleUnitView> GetAllUnitViews()
    {
        if (battle.settings.parallelCount > 1)
            return doublePlayerView.Concat(doubleEnemyView).ToList();
        else
            return new List<BattleUnitView>() { playerView, enemyView };
    }

    public void StartTimer() => systemView.StartTimer();
    public void DoneTimer() => systemView.DoneTimer();
    public void StopTimer() => systemView.StopTimer();
    public void ProcessResult(BattleResult result) => systemView.ProcessResult(result);
    public void OnConfirmBattleResult()
    {
        if (battle.settings.mode == BattleMode.PVP)
        {
            NetworkData data = new NetworkData() { networkAction = NetworkAction.Leave };
            NetworkManager.instance.StartNetworkAction(data);
        }

        if (systemView.ShowPetSecretSkillResult())
            return;

        SceneLoader.instance.ChangeScene(SceneId.Map);
    }

    public void SetState(BattleState lastState, BattleState currentState)
    {
        if (lastState == null)
            queue.Enqueue(null);

        queue.Enqueue(currentState);
    }

    private void SetCurrentUIState(BattleState newState) 
    {
        systemView.SetState(currentUIState, newState);
        allPlayerViews?.ForEach((x, i) => x?.SetState(currentUIState, newState, i));
        allEnemyViews?.ForEach((x, i) => x?.SetState(currentUIState, newState, i));

        currentUIState = newState;
    }

    /// <summary>
    /// Start processing UI Query. <br/>
    /// If processOne = true, then no checking for done or any UI conficts.
    /// </summary>
    public void ProcessQuery(bool processOne = false)
    {
        if (queue.Count <= 0)
        {
            if (currentUIState == null)
                return;

            SetBottomBarInteractable(true);
            SelectOption(currentUIState.myUnit.pet.isDead ? 1 : 0);
            SetOptionActive(2, currentUIState.settings.isCaptureOK);
                
            if ((!currentUIState.settings.isItemOK) || currentUIState.myUnit.pet.isDead)
            {
                if (currentUIState.myUnit.pet.isDead)
                    SetOptionActive(0, false);
                    
                SetOptionActive(2, false);
                SetOptionActive(3, false);
            }

            CheckAutoSkill();

            if (battle.settings.mode == BattleMode.Record) {
                var actionList = Player.instance.currentBattleRecord.actionList;
                while (recordActionIndex < actionList.Count) {
                    var action = actionList[recordActionIndex++];
                    var skill = Skill.ParseRPCData(action.key);
                    if (battle.SetSkill(skill, action.value))
                        break;
                }
            }
            
            return;
        }

        var newState = queue.Dequeue();
        if (newState == null)
        {
            currentUIState = null;
            newState = queue.Dequeue();
            if (newState.result.isBattleEnd)
            {
                allPlayerViews?.ForEach((x, i) => x?.SetState(currentUIState, newState, i));
                allEnemyViews?.ForEach((x, i) => x?.SetState(currentUIState, newState, i));
                ProcessResult(newState.result);
                return;
            }
        }

        SetCurrentUIState(newState);

        if (processOne) 
        {
            if ((newState.settings.parallelCount > 1) && (queue.Count <= 0)) 
            {
                SetBottomBarInteractable(newState.myUnit.skillSystem.skill == null);
                SelectOption(newState.myUnit.pet.isDead ? 1 : 0);
                SetOptionActive(2, newState.settings.isCaptureOK);
    
                if ((!newState.settings.isItemOK) || (newState.myUnit.pet.isDead))
                {
                    SetOptionActive(2, false);
                    SetOptionActive(3, false);
                }

                SetOptionActive(4, !newState.myUnit.pet.isDead);
            }
            return;
        }

        StartCoroutine(CheckQueryDoneCoroutine(newState));
    }

    protected IEnumerator CheckQueryDoneCoroutine(BattleState newState)
    {
        while (!isDone)
        {
            yield return new WaitForSeconds(0.2f);
        }

        // if (newState.settings.mode == BattleMode.Record)
        //     yield return new WaitForSeconds(1f);

        ProcessQuery();
    }

    public void CheckAutoSkill() {
        if (queue.Count > 0)
            return;
        
        StartCoroutine(CheckAutoSkillCoroutine());
    }

    protected IEnumerator CheckAutoSkillCoroutine() 
    {
        // PVP和双打不适用自动战斗
        if (!currentUIState.settings.isAutoOK)
            yield break;

        systemView.SetAutoBattleActive(false);
        if (ListHelper.IsNullOrEmpty(battle.autoSkillOrder))
            yield break;

        var cursor = battle.autoSkillOrder.Get(battle.autoSkillCursor);
        var pet = currentUIState.myUnit.pet;

        if (pet.isDead) {
            systemView.StopAutoBattle();
            yield break;
        }

        var isNormalSkill = cursor.IsWithin(0, 4);
        var item = isNormalSkill ? null : new Item(cursor, (Item.Find(cursor) == null) ? 0 : 1);
        var skill = isNormalSkill ? (pet.normalSkill.Get(cursor - 1) ?? Skill.GetNoOpSkill()) : Skill.GetItemSkill(item);
        var superSkill = pet.superSkill;
        var isAnySkillOK = pet.buffController.GetBuff(61) != null;
        var isSuper = battle.isAutoSuperSkill && (superSkill != null)
            && ((superSkill.anger <= pet.anger) || isAnySkillOK);

        if (!isNormalSkill)
        {
            var usable = item.IsUsable(battle?.currentState.myUnit, battle?.currentState);
            var isTimingOk = item.effects.Exists(x => (x.timing >= EffectTiming.OnBattleStart) && (x.timing <= EffectTiming.OnBattleEnd));
            var isCaptureOK = battle.settings.isCaptureOK || (item.info.type != ItemType.Capture);
            if (usable && isTimingOk && isCaptureOK)
            {
                if (!battle.settings.isSimulate)
                    Item.Remove(item.id);
            }
            else
                skill = Skill.GetNoOpSkill();
        }

        if (isSuper)
            skill = superSkill;
        else if ((skill.anger > pet.anger) && (!isAnySkillOK))
            skill = Skill.GetNoOpSkill();

        systemView.SetAutoBattleActive(true);
        if (!isSuper)
            battle.autoSkillCursor = (battle.autoSkillCursor + 1) % battle.autoSkillOrder.Count;
            
        battle.SetSkill(skill, true);
    }

    public void SetSkillSelectMode(bool isSkillSelectMode) {
        this.isSkillSelectMode = isSkillSelectMode;
        systemView.SetSkillSelectMode(isSkillSelectMode);

        if (!isSkillSelectMode)
            SetCurrentUIState(currentUIState);
    }

    public void SelectOption(int index) => systemView.SelectOption(index);
    public void SetOptionActive(int index, bool active) => systemView.SetOptionActive(index, active);
    public void SetBottomBarInteractable(bool interactable) => systemView.SetBottomBarInteractable(interactable);

    public void SetBattleEscape()
    {
        var skill = Skill.GetEscapeSkill();
        if (battle.settings.parallelCount > 1)
            skill.SetParallelIndex(currentUIState.myUnit.petSystem.cursor, currentUIState.opUnit.petSystem.cursor);

        battle.SetSkill(skill, true);
    }

    protected void OpenDisconnectHintbox(string message)
    {
        Hintbox hintbox = Hintbox.OpenHintbox();
        hintbox.SetTitle("提示");
        hintbox.SetContent(message, 16, FontOption.Arial);
        hintbox.SetOptionNum(1);
        hintbox.SetOptionCallback(OnConfirmBattleResult);
    }

    public void OnLocalPlayerDisconnect(DisconnectCause disconnectCause, string failedMessage)
    {
        if (battle.result.isBattleEnd)
            return;

        OpenDisconnectHintbox("我方连线已中断");
        NetworkManager.instance.onDisconnectEvent -= OnLocalPlayerDisconnect;
    }

    public void OnOtherPlayerDisconnect(Photon.Realtime.Player player)
    {
        if (battle.result.isBattleEnd)
            return;

        OpenDisconnectHintbox("对手连线已中断");
        NetworkManager.instance.onOtherPlayerLeftRoomEvent -= OnOtherPlayerDisconnect;
    }

    public void PVPSetSkillToOthers(Skill skill)
    {
        if (battle.settings.mode != BattleMode.PVP)
            return;

        var photonView = masterView.IsMine ? masterView : clientView;
        photonView.RPC("RPCSetSkill", RpcTarget.Others, (object)skill.ToRPCData(battle.settings));
        Player.instance.gameData.battleRecordStorage?.LastOrDefault()?.AddAction(skill.ToRPCData(battle.settings), true);
    }
}