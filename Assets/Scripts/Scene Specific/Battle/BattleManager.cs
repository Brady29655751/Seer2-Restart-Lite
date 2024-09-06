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

    public void StartTimer()
    {
        systemView.StartTimer();
    }

    public void DoneTimer()
    {
        systemView.DoneTimer();
    }

    public void StopTimer()
    {
        systemView.StopTimer();
    }

    public void ProcessResult(BattleResult result)
    {
        systemView.ProcessResult(result);
    }

    public void OnConfirmBattleResult()
    {
        if (battle.settings.mode == BattleMode.PVP)
        {
            NetworkData data = new NetworkData() { networkAction = NetworkAction.Leave };
            NetworkManager.instance.StartNetworkAction(data);
        }

        SceneLoader.instance.ChangeScene(SceneId.Map);
    }

    public void SetState(BattleState lastState, BattleState currentState)
    {
        if (lastState == null)
            queue.Enqueue(null);

        queue.Enqueue(currentState);
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

        systemView.SetState(currentUIState, newState);
        playerView.SetState(currentUIState, newState);
        enemyView.SetState(currentUIState, newState);

        currentUIState = newState;

        if (processOne)
            return;

        StartCoroutine(CheckQueryDone(newState));
    }

    protected IEnumerator CheckQueryDone(BattleState newState)
    {
        while (!isDone)
        {
            yield return new WaitForSeconds(0.2f);
        }

        ProcessQuery();
    }

    public void SelectOption(int index)
    {
        systemView.SelectOption(index);
    }

    public void SetOptionActive(int index, bool active)
    {
        systemView.SetOptionActive(index, active);
    }

    public void SetBottomBarInteractable(bool interactable)
    {
        systemView.SetBottomBarInteractable(interactable);
    }

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
        photonView.RPC("RPCSetSkill", RpcTarget.Others, (object)skill.ToRPCData());
    }
}