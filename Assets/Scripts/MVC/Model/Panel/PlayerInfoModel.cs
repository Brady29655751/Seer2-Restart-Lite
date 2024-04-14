using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInfoModel : Module
{
    private GameData data => Player.instance.gameData;
    public int coin => data.coin;
    public int diamond => data.diamond;

    public void OnChangeNameSuccess(string newName) {
        data.nickname = newName;
        SaveSystem.SaveData();
    }

}
