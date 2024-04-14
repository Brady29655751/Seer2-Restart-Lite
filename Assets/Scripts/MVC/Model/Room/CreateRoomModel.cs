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
        mode = BattleMode.PVP,
    };

    public void SetPetCount(int count) {
        settings.petCount = count;
    }

    public void CreateRoom() {
        NetworkData networkData = new NetworkData() {
            networkAction = NetworkAction.Create,
            roomName = Random.Range(10001, 100000).ToString(),
        };
        networkData.roomProperty["count"] = settings.petCount;
        NetworkManager.instance.StartNetworkAction(networkData);
    }
}
