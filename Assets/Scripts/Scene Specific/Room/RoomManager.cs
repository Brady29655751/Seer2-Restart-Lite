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

    public EffectTiming phase = EffectTiming.OnTurnStart;
    private List<int> banPetList = new List<int>(), opBanPetList = new List<int>();


    protected override void Awake()
    {
        base.Awake();
        InitSubscriptions();
    }

    private void Start()
    {
        InitPlayer();
        SetMyReadyProperty(false);
    }

    private void InitSubscriptions()
    {
        NetworkManager.instance.onPlayerPropsUpdateEvent += OnPlayerReady;
        NetworkManager.instance.onOtherPlayerJoinedRoomEvent += OnOtherPlayerJoin;
        NetworkManager.instance.onOtherPlayerLeftRoomEvent += OnOtherPlayerLeft;
    }

    private void RemoveSubscriptions()
    {
        NetworkManager.instance.onPlayerPropsUpdateEvent -= OnPlayerReady;
        NetworkManager.instance.onOtherPlayerJoinedRoomEvent -= OnOtherPlayerJoin;
        NetworkManager.instance.onOtherPlayerLeftRoomEvent -= OnOtherPlayerLeft;
    }

    private void InitPlayer()
    {
        phase = EffectTiming.OnTurnStart;

        petBagPanel.SetActive(true);
        petBagPanel.SetActive(false);

        var room = PhotonNetwork.CurrentRoom.CustomProperties;
        var player = PhotonNetwork.LocalPlayer.CustomProperties;
        int seed = Random.Range(1, int.MaxValue);
        int petCount = (int)(room["count"]);
        bool isItemOK = (bool)(room["item"]);
        int[] buffs = (int[])(room["buff"]);

        if (NetworkManager.IsMasterClient)
        {
            room["seed"] = seed;
            PhotonNetwork.CurrentRoom?.SetCustomProperties(room);
        }

        var myPets = Player.instance.petBag.Take(petCount).Select(x => (x == null) ? null : new Pet(x)).Select(x =>
        {
            if (x == null)
                return x;

            if (x.level > 100)
            {
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
            if (x.talent.ev.sum > 510)
            {
                x.talent.SetEV(Status.zero);
                x.talent.SetEVStorage(510);
            }
            return x;
        }).Select(x => (x == null) ? null : Pet.ToBestPet(new Pet(x), buffs.Contains(Buff.BUFFID_PVP_IV_120) ? 120 : 31));

        petBagPanel.SetPetBag(myPets.ToArray());
        petBagPanel.SetItemBag(isItemOK ? Item.pvpItemDatabase : new List<Item>() { new Item(10239, 9999), new Item(10240, 9999) });
        roomSettingsView.SetPet(myPets.ToList(), true);
        roomSettingsView.SetPet(Enumerable.Repeat(Pet.GetExamplePet(0), petCount).ToList(), false);
    }

    public void LeaveRoom()
    {
        RemoveSubscriptions();
        SetMyReadyProperty(false);
        NetworkData networkData = new NetworkData()
        {
            networkAction = NetworkAction.Leave,
        };
        NetworkManager.instance.StartNetworkAction(networkData);
    }

    private void OnOtherPlayerJoin(Photon.Realtime.Player player)
    {
        roomSettingsView.SetName(player.NickName, false);
    }

    private void OnOtherPlayerLeft(Photon.Realtime.Player player)
    {
        if (player.IsInactive)
            return;

        var room = PhotonNetwork.CurrentRoom.CustomProperties;
        var bp = (BattleBanPick)(int)room["bp"];
        if (bp != BattleBanPick.None)
        {
            var hintbox = Hintbox.OpenHintboxWithContent("对手连线已中断", 16);
            hintbox.SetOptionNum(1);
            hintbox.SetOptionCallback(LeaveRoom);
            return;   
        }

        NetworkManager.IsMasterClient = true;
        roomSettingsView.SetName(string.Empty, false);
    }

    private void OnPlayerReady(Photon.Realtime.Player player)
    {
        var allPlayers = PhotonNetwork.PlayerList;
        var otherPlayer = PhotonNetwork.PlayerListOthers;
        var room = PhotonNetwork.CurrentRoom.CustomProperties;
        var bp = (BattleBanPick)(int)room["bp"];
        var hash = player.CustomProperties;
        if ((!player.IsLocal) && (bool)hash["ready"])
        {
            roomSettingsView.SetReady(null, true, false);
        }

        bool isAllReady = (allPlayers.Length > 1) && allPlayers.All(x => (bool)x.CustomProperties["ready"]);
        if (isAllReady)
        {
            opBanPetList = ((int[])otherPlayer[0].CustomProperties["ban"]).ToList();

            SetMyReadyProperty(false);
            NextPhase();
            roomSettingsView.SetPet(((int[])otherPlayer[0].CustomProperties["pet"]).Select(x => Pet.GetExamplePet(x)).ToList(), false);

            if (bp != BattleBanPick.None)
                roomSettingsView.SetBanPick(opBanPetList, petBagPanel.petBag.Take(6).Select(x => x?.id ?? 0).ToList(), true);

            roomSettingsView.OnAllReady(OnAllReady);
        }
    }

    private void NextPhase()
    {
        switch (phase)
        {
            default:
                break;

            case EffectTiming.OnTurnStart:
                phase = EffectTiming.OnTurnReady;
                break;

            case EffectTiming.OnTurnReady:
                phase = EffectTiming.OnAttackStart;
                break;

            case EffectTiming.OnAttackStart:
                phase = EffectTiming.OnAttackReady;
                break;
        }
    }

    private void OnAllReady()
    {
        var otherPlayer = PhotonNetwork.PlayerListOthers;
        Battle battle = new Battle
        (
            PhotonNetwork.CurrentRoom.CustomProperties,
            PhotonNetwork.LocalPlayer.CustomProperties,
            otherPlayer[0].CustomProperties
        );
        StartBattle();
    }

    private void SetMyReadyProperty(bool isReady)
    {
        var hash = PhotonNetwork.LocalPlayer.CustomProperties;
        hash["ready"] = isReady;

        if (!isReady)
        {
            PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
            return;
        }

        hash["pet"] = petBagPanel.petBag.Where(x => x != null).Select(x => x.id).ToArray();
        hash["skin"] = petBagPanel.petBag.Where(x => x != null).Select(x => (int)x.GetPetIdentifier("skinId")).ToArray();
        hash["char"] = petBagPanel.petBag.Where(x => x != null).Select(x => (int)x.basic.personality).ToArray();
        hash["feature"] = petBagPanel.petBag.Where(x => x != null).Select(x => x.feature.featureId).ToArray();
        hash["emblem"] = petBagPanel.petBag.Where(x => x != null).Select(x => x.feature.emblemId).ToArray();
        hash["buff"] = petBagPanel.petBag.Where(x => x != null).Select(x => x.feature.afterwardBuffIds.ToArray()).ToArray();
        hash["ev"] = petBagPanel.petBag.Where(x => x != null).Select(x => x.talent.ev.ToArray()).ToArray();
        hash["skill"] = petBagPanel.petBag.Where(x => x != null).Select(x => x.normalSkill.Select(x => x?.id ?? 0).ToArray()).ToArray();
        hash["super"] = petBagPanel.petBag.Where(x => x != null).Select(x => x.superSkill?.id ?? 0).ToArray();
        hash["resist"] = petBagPanel.petBag.Where(x => x != null).Select(x => x.resist.ToArray()).ToArray();
        hash["ban"] = banPetList.ToArray();

        PhotonNetwork.LocalPlayer.SetCustomProperties(hash);

        return;
    }

    public void SetMyReady(bool isReady)
    {
        var room = PhotonNetwork.CurrentRoom.CustomProperties;
        var otherPlayer = PhotonNetwork.PlayerListOthers;
        var buffs = (int[])room["buff"];
        var bp = (BattleBanPick)(int)room["bp"];

        if (isReady)
        {
            // Check rule.
            var pets = petBagPanel.petBag.Where(x => x != null);
            if (buffs.Contains(610001))
            {
                if (pets.Count() < 6)
                {
                    Hintbox.OpenHintboxWithContent("【无禽12星限制】\n必须带满6只精灵", 16);
                    return;
                }
                if (pets.Any(x => x.info.star >= 4))
                {
                    Hintbox.OpenHintboxWithContent("【无禽12星限制】\n不能使用4星以上的精灵", 16);
                    return;
                }
                if (pets.Count(x => x.info.star == 3) > 2)
                {
                    Hintbox.OpenHintboxWithContent("【无禽12星限制】\n3星精灵最多携带2只", 16);
                    return;
                }
                if (pets.Sum(x => x.info.star) > 12)
                {
                    Hintbox.OpenHintboxWithContent("【无禽12星限制】\n精灵星数总和不能超过12", 16);
                    return;
                }
            }
            if (buffs.Contains(Buff.BUFFID_PET_EXCHANGE))
            {
                if (pets.Count() != 3)
                {
                    Hintbox.OpenHintboxWithContent("【精灵大乱斗限制】\n必须且只能携带3只精灵", 16);
                    return;
                }
            }

            if (bp != BattleBanPick.None)
            {
                if (pets.Count() != pets.Select(x => x.basic.baseId).Distinct().Count())
                {
                    Hintbox.OpenHintboxWithContent("【BP模式限制】\n不能携带具有相同<color=#ffbb33>基础序号</color>的精灵", 16);
                    return;
                }

                if (pets.Count() != pets.Select(x => x.GetPetIdentifier("id")).Distinct().Count())
                {
                    Hintbox.OpenHintboxWithContent("【BP模式限制】\n不能携带具有相同<color=#ffbb33>序号</color>的精灵", 16);
                    return;
                }

                if ((bp == BattleBanPick.Ranking) && (phase == EffectTiming.OnAttackStart))
                {
                    if (pets.Take(6).Any(x => opBanPetList.Contains(x.id)))
                    {
                        Hintbox.OpenHintboxWithContent("【竞技模式限制】\n不能使用对手已禁用的精灵", 16);
                        return;
                    }   
                }
            }
            roomSettingsView.SetReady(() => SetMyReadyProperty(isReady), isReady, true);
        }
    }

    public void StartBattle()
    {
        SceneLoader.instance.ChangeScene(SceneId.Battle, true);
    }

    public void BanPet(int index)
    {
        var room = PhotonNetwork.CurrentRoom.CustomProperties;
        var otherPlayer = PhotonNetwork.PlayerListOthers;
        var bp = (BattleBanPick)(int)room["bp"];
        var hash = otherPlayer[0].CustomProperties;
        var pets = ((int[])hash["pet"]).ToList();

        switch (bp)
        {
            default:
                return;

            case BattleBanPick.Ranking:
                if ((pets.Get(index) == 0) || (phase != EffectTiming.OnTurnReady))
                    return;
                
                banPetList.Flip(pets[index]);
                if (banPetList.Count > 3)
                {
                    banPetList.Flip(pets[index]);
                    Hintbox.OpenHintboxWithContent("【竞技模式限制】\n只能禁用3只精灵", 16);
                    return;
                }
                roomSettingsView.SetBanPick(banPetList, null, false);
                return;
        }
    }

    public void OpenPetBag()
    {
        petBagPanel.SetActive(true);
    }

    public void ClosePetBag()
    {
        roomSettingsView.SetPet(petBagPanel.petBag.ToList(), true);
        petBagPanel.SetActive(false);

        var room = PhotonNetwork.CurrentRoom.CustomProperties;
        var bp = (BattleBanPick)(int)room["bp"];
        if (bp != BattleBanPick.None)
            roomSettingsView.SetBanPick(opBanPetList, petBagPanel.petBag.Take(6).Select(x => x?.id ?? 0).ToList(), true);
    }

    public void SetPetStorageActive(bool active)
    {
        petBagPanel.RefreshPetBag();
        petStoragePanel.SetActive(active);
    }
}
