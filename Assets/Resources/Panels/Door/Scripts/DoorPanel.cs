using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorPanel : Panel
{
    [SerializeField] private DoorController doorController;

    public static string[] doorNames => new string[] { "challenge", "brave", "competition", "hero", "twin", "king", "lonely" };

    public override void SetPanelIdentifier(string id, string param) {
        switch (id) {
            default:
                base.SetPanelIdentifier(id, param);
                break;
            case "door":
                doorController.SetDoor(param);
                break;
            case "mode":
                doorController.SetMode(param);
                break;
        }
    }
    
}
