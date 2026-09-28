using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class RoomSettingsView : Module
{
    [SerializeField] private Timer timer;
    [SerializeField] private Text roomNumText, turnTimeText, banPickText;
    [SerializeField] private BattlePetBuffView stateBuffView;
    [SerializeField] private IButton startButton, petBagButton;
    [SerializeField] private Text myNameText, opNameText;
    [SerializeField] private List<PetSelectBlockView> myPets, opPets, myRankingPets, opRankingPets, opBanPets;
    [SerializeField] private List<Image> myPetStarFrames, opPetStarFrames, myRankingPetStarFrames, opRankingPetStarFrames;
    [SerializeField] private GameObject stateVSObject, myConfirmObject, myCancelObject, opCoverObject, opBanObject;
    [SerializeField] private GameObject myNormalPetBag, opNormalPetBag, myRankingPetBag, opRankingPetBag;
    [SerializeField] private RectTransform myBottomTransform;
    [SerializeField] private Text opStatusText;

    public override void Init()
    {
        base.Init();
        InitRoom();
    }

    private void InitRoom()
    {
        var hash = PhotonNetwork.CurrentRoom.CustomProperties;
        var otherPlayers = PhotonNetwork.PlayerListOthers;
        var buffs = ((int[])hash["buff"]).Select(x => new Buff(x)).ToList();
        var reveal = (bool)hash["reveal"];
        var bp = (BattleBanPick)(int)hash["bp"];
        banPickText?.SetText(bp.ToChineseName("房间号码"));
        roomNumText?.SetText(PhotonNetwork.CurrentRoom.Name);
        turnTimeText?.SetText("【" + hash["time"] + " 秒】");
        stateBuffView?.SetBuff(buffs);
        stateBuffView?.gameObject?.SetActive(buffs.Count > 0);
        stateVSObject?.SetActive(buffs.Count <= 0);
        opCoverObject?.SetActive((!reveal) && (bp == BattleBanPick.None));
        myNormalPetBag?.SetActive(bp != BattleBanPick.Ranking);
        opNormalPetBag?.SetActive(bp != BattleBanPick.Ranking);
        myRankingPetBag?.SetActive(bp == BattleBanPick.Ranking);
        opRankingPetBag?.SetActive(bp == BattleBanPick.Ranking);
        myBottomTransform.anchoredPosition = (bp == BattleBanPick.Ranking) ? new Vector2(290, 0) : Vector2.zero;
        opStatusText?.gameObject.SetActive(bp == BattleBanPick.None);
        timer?.gameObject.SetActive(bp == BattleBanPick.None);
        // opBanObject?.SetActive(bp != BattleBanPick.None);
        SetName(PhotonNetwork.LocalPlayer.NickName, true);
        SetName((otherPlayers.Length == 0) ? null : otherPlayers[0].NickName, false);
        SetBGM();
    }

    public void SetBGM()
    {
        ResourceManager.instance.GetLocalAddressables<AudioClip>("BGM/1/MU_011", onSuccess: (bgm) =>
        {
            AudioSystem.instance.PlayMusic(bgm, AudioVolumeType.BGM);
        });
    }

    public void SetName(string name, bool isMe)
    {
        var text = isMe ? myNameText : opNameText;
        text?.SetText(name);
    }

    public void SetPet(List<Pet> pets, bool isMe)
    {
        var hash = PhotonNetwork.CurrentRoom.CustomProperties;
        var bp = (BattleBanPick)(int)hash["bp"];
        List<PetSelectBlockView> petBlocks;
        List<Image> starFrames;

        if (isMe)
        {
            petBlocks = (bp == BattleBanPick.Ranking) ? myRankingPets : myPets;
            starFrames = (bp == BattleBanPick.Ranking) ? myRankingPetStarFrames : myPetStarFrames;
        }
        else
        {
            petBlocks = (bp == BattleBanPick.Ranking) ? opRankingPets : opPets;
            starFrames = (bp == BattleBanPick.Ranking) ? opRankingPetStarFrames : opPetStarFrames;
        }

        for (int i = 0; i < petBlocks.Count; i++)
        {
            var pet = ((pets != null) && (i < pets.Count)) ? pets[i] : null;
            petBlocks[i]?.SetPet(pet);

            if (bp == BattleBanPick.Ranking)
                starFrames[i]?.SetColor(PetInfo.IsMod(pet?.id ?? 0) ? ColorHelper.orange : Color.clear);
            else
                starFrames[i]?.SetColor(ColorHelper.GetStarColor(pet?.info?.star ?? 0));
        }
    }

    public void SetBanPick(List<int> banned, List<int> picked, bool isMe)
    {
        var petBlocks = isMe ? myRankingPets : opRankingPets;
        foreach (var block in petBlocks)
        {
            block.SetBanPick(banned, picked);
        }
    }

    public void SetReady(Action callback, bool isReady, bool isMe)
    {
        if (isMe)
            SetMyReady(callback, isReady);
        else
            SetOpReady(callback, isReady);
    }

    private void SetMyReady(Action callback, bool isReady)
    {
        var hash = PhotonNetwork.CurrentRoom.CustomProperties;
        var bp = (BattleBanPick)(int)hash["bp"];
        var phase = RoomManager.instance.phase;

        void IsConfirmedReady()
        {
            petBagButton?.SetInteractable(false);
            myConfirmObject?.SetActive(!isReady);
            myCancelObject?.SetActive(isReady);
            callback?.Invoke();
        }

        Hintbox hintbox = Hintbox.OpenHintbox();
        hintbox.SetTitle("提示");

        if (isReady)
        {
            switch (bp)
            {
                default:
                    hintbox.SetContent("确定之后就无法更改出战精灵\n确定准备完成吗？", 16, FontOption.Arial);
                    hintbox.SetOptionNum(2);
                    hintbox.SetOptionCallback(IsConfirmedReady);   
                    break;
                    
                case BattleBanPick.Ranking:
                    if (phase == EffectTiming.OnTurnStart)
                        goto default;
                    
                    if (phase == EffectTiming.OnTurnReady)
                    {
                        hintbox.SetContent("确定之后就无法更改禁用精灵\n确定禁用完成吗？", 16, FontOption.Arial);
                        hintbox.SetOptionNum(2);
                        hintbox.SetOptionCallback(IsConfirmedReady);   
                        break;
                    }

                    if (phase == EffectTiming.OnAttackStart)
                        goto default;
                    
                    break;
            }
        }
        else
        {
            IsConfirmedReady();
        }
    }

    private void SetOpReady(Action callback, bool isReady)
    {
        opStatusText?.SetText(isReady ? "准备完成" : "准备中");
        opStatusText?.SetColor(isReady ? ColorHelper.green : ColorHelper.red);

        callback?.Invoke();
    }

    public void OnAllReady(Action callback)
    {
        var hash = PhotonNetwork.CurrentRoom.CustomProperties;
        var bp = (BattleBanPick)(int)hash["bp"];
        var reveal = (bool)hash["reveal"];
        var phase = RoomManager.instance.phase;

        switch (bp)
        {
            default:
                break;

            case BattleBanPick.Ranking:
                if (phase == EffectTiming.OnTurnReady)
                {
                    Hintbox.OpenHintboxWithContent("准备完成，开始禁用精灵环节\n最多禁用对手<b><color=#ffbb33>3位</color></b>精灵", 16);
                    petBagButton?.SetInteractable(false);
                    myConfirmObject?.SetActive(true);
                    myCancelObject?.SetActive(false);   
                    return;
                }

                if (phase == EffectTiming.OnAttackStart)
                {
                    Hintbox.OpenHintboxWithContent("禁用完成，开始出战精灵环节", 16);
                    petBagButton?.SetInteractable(true);
                    myConfirmObject?.SetActive(true);
                    myCancelObject?.SetActive(false);   
                    return; 
                }

                if (phase == EffectTiming.OnAttackReady)
                    break;

                return;
        }

        if ((!reveal) || (bp != BattleBanPick.None))
        {
            callback?.Invoke();
            return;
        }

        timer.onDoneEvent += (leftSeconds) => callback?.Invoke();
        timer.SetTimer(10);
    }
}
