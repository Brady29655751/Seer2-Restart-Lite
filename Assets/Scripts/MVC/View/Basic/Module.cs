using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Module : IMonoBehaviour
{
    public void OpenPanel(string panelName) => Panel.OpenPanel(panelName);
    public void GoToMap(int mapId) => TeleportHandler.Teleport(mapId);
}
