using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoginRoleModel : Module
{
    [SerializeField] private int id = 0;
    public GameData data { get; private set; }
    public string nickname => data.nickname;

    public int GetId() {
        return id;
    }

    public void SetData(GameData data) {
        this.data = data;
    }

}
