using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleFileBrowser;

public class LoginRoleController : Module
{
    [SerializeField] private LoginRoleModel roleModel;
    [SerializeField] private LoginRoleView roleView;

    public void SetData(GameData data) {
        roleModel.SetData(data);
        roleView.SetName(roleModel.nickname);
    }

    public void SetChosen(bool chosen) {
        roleView.SetChosen(chosen);
    }

    public bool Login(out string error) {
        error = string.Empty;

        if (roleModel.data.IsEmpty())
            return false;
        
        Player.instance.gameData = roleModel.data;
        GameManager.instance.ChangeState(GameState.Login);
        return true;
    }
}
