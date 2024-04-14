using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SPTBossPanel : Panel
{
    public void GoToMap(int mapId) {
        TeleportHandler.Teleport(mapId);
    }
}
