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
    [SerializeField] private IText roomNumText;
    [SerializeField] private Text turnTimeText;
    [SerializeField] private IButton startButton, petBagButton;
    [SerializeField] private Text myNameText, opNameText;
    [SerializeField] private List<PetSelectBlockView> myPets, opPets;
    [SerializeField] private GameObject myConfirmObject, myCancelObject, opCoverObject;
    [SerializeField] private IText opStatusText;

    public override void Init()
    {
        base.Init();
        InitRoom();
    }

    private void InitRoom() {
        var hash = PhotonNetwork.CurrentRoom.CustomProperties;
        var otherPlayers = PhotonNetwork.PlayerListOthers;
        roomNumText?.SetText(PhotonNetwork.CurrentRoom.Name);
        turnTimeText?.SetText("【" + hash["time"] + " 秒】");
        SetName(PhotonNetwork.LocalPlayer.NickName, true);
        SetName((otherPlayers.Length == 0) ? null : otherPlayers[0].NickName, false);
        SetBGM(ResourceManager.instance.GetLocalAddressables<AudioClip>("BGM/1/MU_011"));
        // SetPet(Enumerable.Repeat(Pet.GetExamplePet(3), (int)hash["count"]).ToList(), true);
    }

    public void SetBGM(AudioClip bgm) {
        AudioSystem.instance.PlayMusic(bgm, AudioVolumeType.BGM);
    }

    public void SetName(string name, bool isMe) {
        var text = isMe ? myNameText : opNameText;
        text?.SetText(name);
    }

    public void SetPet(List<Pet> pets, bool isMe) {
        var petBlocks = isMe ? myPets : opPets;
        for (int i = 0; i < petBlocks.Count; i++) {
            petBlocks[i]?.SetPet(((pets != null) && (i < pets.Count)) ? pets[i] : null);
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

    public void OnAllReady(bool isAllReady) {
        opCoverObject?.SetActive(!isAllReady);
        startButton?.gameObject.SetActive(PhotonNetwork.IsMasterClient);
        startButton?.SetInteractable(isAllReady);
        
    }
}
