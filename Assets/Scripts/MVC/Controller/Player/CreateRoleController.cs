using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateRoleController : Module
{
    [SerializeField] private CreateRoleModel createModel;
    [SerializeField] private CreateRoleView createView;

    public void SetActive(bool active) {
        createView.SetActive(active);
    }

    public void SetGender(bool gender) {
        createModel.SetGender(gender);
        createView.SetGender(gender);
    }

    public bool CreateRole() {
        if (string.IsNullOrEmpty(createModel.inputString)) {
            createView.OnSetPlayerNameEmpty();
            return false;
        }
        Player.instance.gameData = GameData.GetDefaultData(2000, 0);
        Player.instance.gameData.gender = createModel.gender;
        Player.instance.gameData.nickname = createModel.inputString;
        Mail.DailyLogin();
        SaveSystem.SaveData();
        return true;
    }

}
