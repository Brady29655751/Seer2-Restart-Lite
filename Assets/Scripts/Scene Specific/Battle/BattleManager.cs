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
    [SerializeField] private PhotonView masterView, clientView;

    public bool isDone => playerView.isDone && enemyView.isDone;
    public bool isSkillSelectMode { get; protected set; } = false;
    protected BattleState currentUIState = null;
    protected Queue<BattleState> queue = new Queue<BattleState>();

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
        playerView.SetState(currentUIState, newState);
        enemyView.SetState(currentUIState, newState);

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
            SetBottomBarInteractable(true);
            SelectOption(currentUIState.myUnit.pet.isDead ? 1 : 0);
            SetOptionActive(2, currentUIState.settings.isCaptureOK);
                
            if ((!currentUIState.settings.isItemOK) || (currentUIState.myUnit.pet.isDead))
            {
                SetOptionActive(2, false);
                SetOptionActive(3, false);
            }

            CheckAutoSkill();
            return;
        }

        var newState = queue.Dequeue();
        if (newState == null)
        {
            currentUIState = null;
            newState = queue.Dequeue();
            if (newState.result.isBattleEnd)
            {
                playerView.SetState(currentUIState, newState);
                enemyView.SetState(currentUIState, newState);
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
        systemView.SetAutoBattleActive(false);
        if ((currentUIState.settings.mode == BattleMode.PVP) || (currentUIState.settings.parallelCount > 1))
            yield break;

        if (ListHelper.IsNullOrEmpty(battle.autoSkillOrder))
            yield break;

        var cursor = battle.autoSkillOrder.Get(battle.autoSkillCursor);
        var pet = currentUIState.myUnit.pet;
        var skill = pet.normalSkill.Get(cursor - 1) ?? Skill.GetNoOpSkill();

        if (pet.isDead) {
            systemView.StopAutoBattle();
            yield break;
        }

        if ((skill.anger > pet.anger) && (pet.buffController.GetBuff(61) == null))
            skill = Skill.GetNoOpSkill();

        systemView.SetAutoBattleActive(true);
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
        battle.SetSkill(Skill.GetEscapeSkill(), true);
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
    }
}