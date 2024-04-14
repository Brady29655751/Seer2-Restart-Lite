using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoginManager : Manager<LoginManager>
{
    [SerializeField] private LoginController loginController;
    [SerializeField] private CreateRoleController createController;
    [SerializeField] private ImportRoleController importController;

    private void Start()
    {
        Init();
    }

    public void Init() {
        SetAllGameData();
        InitCreateRoleController();
    }

    private void SetAllGameData() {
        for (int i = 0; i < 3; i++) {
            loginController.SetData(i, SaveSystem.LoadData(i));
        }
    }

    private void InitCreateRoleController() {
        createController.SetActive(true);
        createController.SetActive(false);
    }

    public void Login(int id) {
        if (importController.isImporting)
            return;

        if (!id.IsInRange(0, 3))
            return;

        Player.instance.gameDataId = id;
        
        if (loginController.Login(id)) {
            SceneLoader.instance.ChangeScene(SceneId.Map);
            return;
        }
        createController.SetActive(true);
    }

    public void CreateRole() {
        if (!createController.CreateRole())
            return;
            
        loginController.SetData(Player.instance.gameDataId, Player.instance.gameData);
        Login(Player.instance.gameDataId);
    }

    public void ImportRole() {
        importController.Import();
    }
    
}
