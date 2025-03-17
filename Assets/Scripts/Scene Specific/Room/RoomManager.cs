using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using UnityEngine;
using Random = UnityEngine.Random;

public class RoomManager : Manager<RoomManager>
{
    [SerializeField] private RoomSettingsView roomSettingsView;
    [SerializeField] private PetBagPanel petBagPanel;
    [SerializeField] private PetStoragePanel petStoragePanel;

    protected override void Awake()
    {
        base.Awake();
        InitSubscriptions();
    }

    private void Start() {
        InitPlayer();
        SetMyReadyProperty(false);
    }

    private void InitSubscriptions() {
        NetworkManager.instance.onPlayerPropsUpdateEvent += OnPlayerReady;
        NetworkManager.instance.onOtherPlayerJoinedRoomEvent += OnOtherPlayerJoin;
        NetworkManager.instance.onOtherPlayerLeftRoomEvent += OnOtherPlayerLeft;
    }

    private void RemoveSubscriptions() {
        NetworkManager.instance.onPlayerPropsUpdateEvent -= OnPlayerReady;
        NetworkManager.instance.onOtherPlayerJoinedRoomEvent -= OnOtherPlayerJoin;
        NetworkManager.instance.onOtherPlayerLeftRoomEvent -= OnOtherPlayerLeft;
    }

    private void InitPlayer() {
        petBagPanel.SetActive(true);
        petBagPanel.SetActive(false);

        var room = PhotonNetwork.CurrentRoom.CustomProperties;
        var player = PhotonNetwork.LocalPlayer.CustomProperties;
        int seed = Random.Range(int.MinValue, int.MaxValue);
        int petCount = (int)(room["count"]);
        bool isItemOK = (bool)(room["item"]);
        int[] buffs = (int[])(room["buff"]);

        if (PhotonNetwork.IsMasterClient) {
            room["seed"] = seed;
            PhotonNetwork.CurrentRoom?.SetCustomProperties(room);
        }

        var myPets = Player.instance.petBag.Take(petCount).Select(x => (x == null) ? null : new Pet(x)).Select(x => {
            if (x == null)
                return x;
            
            if (x.level > 100) {
                x.exp.LevelDown(100);
                x.currentStatus = x.normalStatus;
                /*
                var normalSkillId = x.skills.normalSkillId;
                var superSkillId = x.skills.superSkillId;
                x.skills.normalSkillId = normalSkillId.Where(x.skills.ownSkillId.Contains).ToArray();
                x.skills.normalSkillId = x.skills.normalSkillId.Concat(x.skills.backupNormalSkill.Take(4 - x.skills.normalSkillId.Length).Select(x => x.id)).ToArray();
                if (x.skills.ownSkillId.Contains(superSkillId))
                    x.skills.superSkillId = superSkillId;
                */
            }
            // Clear afterward buffs if item is locked.
            if (!isItemOK)
                x.feature.afterwardBuffIds.Clear();

            x.record = new PetRecord();
            if (x.talent.ev.sum > 510) {
                x.talent.SetEV(Status.zero);
                x.talent.SetEVStorage(510);
            }
            return x;
        }).Select(x => (x == null) ? null : Pet.ToBestPet(new Pet(x), buffs.Contains(Buff.BUFFID_PVP_IV_120) ? 120 : 31));
        
        petBagPanel.SetPetBag(myPets.ToArray());
        petBagPanel.SetItemBag(isItemOK ? Item.pvpItemDatabase : new List<Item>(){ new Item(10239, 9999) });
        roomSettingsView.SetPet(myPets.ToList(), true);
    }

    public void LeaveRoom() {
        RemoveSubscriptions();
        SetMyReadyProperty(false);
        NetworkData networkData = new NetworkData() {
            networkAction = NetworkAction.Leave,
        };
        NetworkManager.instance.StartNetworkAction(networkData);
    }

    private void OnOtherPlayerJoin(Photon.Realtime.Player player) {
        roomSettingsView.SetName(player.NickName, false);
        // roomSettingsView.SetPet(((int[])player.CustomProperties["pet"])?.Select(x => Pet.GetExamplePet(x)).ToList(), false);
    }

    private void OnOtherPlayerLeft(Photon.Realtime.Player player) {
        roomSettingsView.SetName(string.Empty, false);
        // roomSettingsView.SetPet(null, false);
    }

    private void OnPlayerReady(Photon.Realtime.Player player) {
        var allPlayers = PhotonNetwork.PlayerList;        
        var otherPlayer = PhotonNetwork.PlayerListOthers;
        var hash = player.CustomProperties;
        if ((!player.IsLocal) && (bool)hash["ready"]) {
            roomSettingsView.SetReady(null, true, false);
            // roomSettingsView.SetPet(((int[])hash["pet"]).Select(x => Pet.GetExamplePet(x)).ToList(), false);
        }

        bool isAllReady = (allPlayers.Length > 1) && allPlayers.All(x => (bool)x.CustomProperties["ready"]);
        if (isAllReady) {
            Battle battle = new Battle(
                PhotonNetwork.CurrentRoom.CustomProperties,
                PhotonNetwork.LocalPlayer.CustomProperties,
                otherPlayer[0].CustomProperties
            );
            StartBattle();
        }
    }

    private void SetMyReadyProperty(bool isReady) {
        var hash = PhotonNetwork.LocalPlayer.CustomProperties;
        hash["ready"] = isReady;

        if (!isReady) {
            PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
            return;
        }

        hash["pet"] = petBagPanel.petBag.Where(x => x != null).Select(x => x.id).ToArray();
        hash["char"] = petBagPanel.petBag.Where(x => x != null).Select(x => (int)x.basic.personality).ToArray();
        hash["feature"] = petBagPanel.petBag.Where(x => x != null).Select(x => x.feature.featureId).ToArray();
        hash["emblem"] = petBagPanel.petBag.Where(x => x != null).Select(x => x.feature.emblemId).ToArray();
        hash["buff"] = petBagPanel.petBag.Where(x => x != null).Select(x => x.feature.afterwardBuffIds.ToArray()).ToArray();
        hash["ev"] = petBagPanel.petBag.Where(x => x != null).Select(x => x.talent.ev.ToArray()).ToArray();
        hash["skill"] = petBagPanel.petBag.Where(x => x != null).Select(x => x.normalSkill.Select(x => x?.id ?? 0).ToArray()).ToArray();
        hash["super"] = petBagPanel.petBag.Where(x => x != null).Select(x => x.superSkill?.id ?? 0).ToArray();
        
        PhotonNetwork.LocalPlayer.SetCustomProperties(hash);

        return;
    }

    public void SetMyReady(bool isReady) {
        roomSettingsView.SetReady(() => SetMyReadyProperty(isReady), isReady, true);
    }

    public void StartBattle() {
        SceneLoader.instance.ChangeScene(SceneId.Battle, true);
    }

    public void OpenPetBag() {
        petBagPanel.SetActive(true);
    }

    public void ClosePetBag() {
        roomSettingsView.SetPet(petBagPanel.petBag.ToList(), true);
        petBagPanel.SetActive(false);
    }

    public void SetPetStorageActive(bool active) {
        petBagPanel.RefreshPetBag();
        petStoragePanel.SetActive(active);
    }
}
