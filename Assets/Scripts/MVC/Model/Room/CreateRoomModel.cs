using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CreateRoomModel : Module
{
    private bool isTeamReveal = false;
    private BattleSettings settings = new BattleSettings() {
        mode = BattleMode.PVP,
        isSimulate = true,
        isEscapeOK = true,
        isCaptureOK = false,

        isItemOK = false,
        petCount = 1,
        time = 10,
    };

    public List<Buff> rules => settings.initBuffs.Select(x => x.Value).ToList();

    public void SetBattleSystem(BattleRule rule)
    {
        settings.rule = rule;
    }

    public void SetPetCount(int count) {
        settings.petCount = count;
    }

    public void SetTurnTime(int time) {
        settings.time = time;
    }

    public void AddRule(int buffId) {
        var count = settings.initBuffs.Count;
        var rule = new KeyValuePair<string, Buff>("rule[" + count + "]", new Buff(buffId));
        settings.initBuffs = settings.initBuffs.Append(rule).ToList();
    }

    public void RemoveRule() {
        var count = settings.initBuffs.Count;
        if (count <= 0)
            return;
        
        settings.initBuffs = settings.initBuffs.Take(count - 1).ToList();
    }

    public void SetItemBagApproval(bool approved) {
        settings.isItemOK = approved;
    }

    public void SetTeamReveal(bool reveal) {
        this.isTeamReveal = reveal;
    }

    public void CreateRoom() {
        NetworkData networkData = new NetworkData()
        {
            networkAction = NetworkAction.Create,
            roomName = Random.Range(10001, 100000).ToString(),
        };
        networkData.roomProperty["rule"] = (int)settings.rule;
        networkData.roomProperty["count"] = settings.petCount;
        networkData.roomProperty["time"] = settings.time;
        networkData.roomProperty["item"] = settings.isItemOK;
        networkData.roomProperty["buff"] = settings.initBuffs.Select(x => x.Value?.id ?? 0).ToArray();
        networkData.roomProperty["reveal"] = isTeamReveal;
        NetworkManager.instance.StartNetworkAction(networkData);
    }
}
