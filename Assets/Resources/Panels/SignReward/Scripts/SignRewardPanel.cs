using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SignRewardPanel : Panel
{
    [SerializeField] private SignRewardController signRewardController;

    public override void SetPanelIdentifier(string id, string param) {
        switch (id) {
            default:
                base.SetPanelIdentifier(id, param);
                return;
            case "activity":
                signRewardController.SetActivity(param);
                return;
        }
    }
}
