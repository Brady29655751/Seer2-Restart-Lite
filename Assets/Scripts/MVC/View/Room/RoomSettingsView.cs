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
    [SerializeField] private Text roomNumText, turnTimeText;
    [SerializeField] private BattlePetBuffView stateBuffView;
    [SerializeField] private IButton startButton, petBagButton;
    [SerializeField] private Text myNameText, opNameText;
    [SerializeField] private List<PetSelectBlockView> myPets, opPets;
    [SerializeField] private List<Image> myPetStarFrames, opPetStarFrames;
    [SerializeField] private GameObject stateVSObject, myConfirmObject, myCancelObject, opCoverObject;
    [SerializeField] private Text opStatusText;

    public override void Init()
    {
        base.Init();
        InitRoom();
    }

    private void InitRoom() {
        var hash = PhotonNetwork.CurrentRoom.CustomProperties;
        var otherPlayers = PhotonNetwork.PlayerListOthers;
        var buffs = ((int[])hash["buff"]).Select(x => new Buff(x)).ToList();
        var reveal = (bool)hash["reveal"];
        roomNumText?.SetText(PhotonNetwork.CurrentRoom.Name);
        turnTimeText?.SetText("【" + hash["time"] + " 秒】");
        stateBuffView?.SetBuff(buffs);
        stateBuffView?.gameObject?.SetActive(buffs.Count > 0);
        stateVSObject?.SetActive(buffs.Count <= 0);
        opCoverObject?.SetActive(!reveal);
        SetName(PhotonNetwork.LocalPlayer.NickName, true);
        SetName((otherPlayers.Length == 0) ? null : otherPlayers[0].NickName, false);
        SetBGM();
    }

    public void SetBGM() {
        ResourceManager.instance.GetLocalAddressables<AudioClip>("BGM/1/MU_011", onSuccess: (bgm) => {
            AudioSystem.instance.PlayMusic(bgm, AudioVolumeType.BGM);
        });
    }

    public void SetName(string name, bool isMe) {
        var text = isMe ? myNameText : opNameText;
        text?.SetText(name);
    }

    public void SetPet(List<Pet> pets, bool isMe) {
        var petBlocks = isMe ? myPets : opPets;
        var starFrames = isMe ? myPetStarFrames : opPetStarFrames;
        for (int i = 0; i < petBlocks.Count; i++) {
            var pet = ((pets != null) && (i < pets.Count)) ? pets[i] : null;
            petBlocks[i]?.SetPet(pet);
            starFrames[i]?.SetColor(ColorHelper.GetStarColor(pet?.info?.star ?? 0));
        }
    }

    public void SetReady(Action callback, bool isReady, bool isMe) {
        if (isMe)
            SetMyReady(callback, isReady);
        else    
            SetOpReady(callback, isReady);
    }

    private void SetMyReady(Action callback, bool isReady) {
        void IsConfirmedReady() {
            petBagButton?.SetInteractable(false);
            myConfirmObject?.SetActive(!isReady);
            myCancelObject?.SetActive(isReady);
            callback?.Invoke();
        }

        Hintbox hintbox = Hintbox.OpenHintbox();
        hintbox.SetTitle("提示");

        if (isReady) {
            hintbox.SetContent("确定之后就无法更改出战精灵\n确定准备完成吗？", 16, FontOption.Arial);
            hintbox.SetOptionNum(2);
            hintbox.SetOptionCallback(IsConfirmedReady);
        } else {
            IsConfirmedReady();
        }
    }

    private void SetOpReady(Action callback, bool isReady) {
        opStatusText?.SetText(isReady ? "准备完成" : "准备中");
        opStatusText?.SetColor(isReady ? ColorHelper.green : ColorHelper.red);

        callback?.Invoke();
    }

    public void OnAllReady(Action callback) {
        var hash = PhotonNetwork.CurrentRoom.CustomProperties;
        var reveal = (bool)hash["reveal"];

        if (!reveal) {
            callback?.Invoke();
            return;
        }
        
        timer.onDoneEvent += (leftSeconds) => callback?.Invoke();
        timer.SetTimer(10);
    }
}
