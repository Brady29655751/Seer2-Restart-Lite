using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateRoomModel : Module
{
    private BattleSettings settings = new BattleSettings() {
        isSimulate = true,
        isEscapeOK = true,
        isCaptureOK = false,
        petCount = 1,
        time = 10,
        mode = BattleMode.PVP,
    };

    public void SetPetCount(int count) {
        settings.petCount = count;
    }

    public void SetTurnTime(int time) {
        settings.time = time;
    }

    public void CreateRoom() {
        NetworkData networkData = new NetworkData() {
            networkAction = NetworkAction.Create,
            roomName = Random.Range(10001, 100000).ToString(),
        };
        networkData.roomProperty["count"] = settings.petCount;
        networkData.roomProperty["time"] = settings.time;
        NetworkManager.instance.StartNetworkAction(networkData);
    }
}
