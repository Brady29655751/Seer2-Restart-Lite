using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoginController : Module
{
    [SerializeField] private LoginRoleController[] roleControllers = new LoginRoleController[3];

    public void SetData(int id, GameData data) {
        if (!id.IsInRange(0, roleControllers.Length))
            return;

        roleControllers[id].SetData(data);
    }

    public bool Login(int id, out string error) {
        if (!id.IsInRange(0, roleControllers.Length)) {
            error = "角色数量不足";
            return false;
        }

        return roleControllers[id].Login(out error);
    }
}
