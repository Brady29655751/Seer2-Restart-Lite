using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class MapManager : Manager<MapManager>
{
    private Map map => Player.instance.currentMap;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private MapSceneController sceneController;
    [SerializeField] private MapUIController UIController;
    [SerializeField] private MapPlantController plantController;

    private void Start()
    {
        Init();
    }

    public void Init() {
        PetTest();
        LoadMap();
        LoadPlayer();
        SetPlantPanelActive(false);
    }

    private void LoadMap() {
        playerController.SetMap(map);
        sceneController.SetMap(map);
        UIController.SetMap(map);
    }

    private void LoadPlayer() {
        playerController.SetPlayerName(Player.instance.nickname);
        playerController.SetPlayerAchievement(Item.GetItemInfo(Player.instance.gameData.achievement)?.name ?? string.Empty);

        Vector2 initPos = (Vector2)Player.GetSceneData("mapInitPos", map.initPoint);
        playerController.SetPlayerPosition(initPos);
        Player.RemoveSceneData("mapInitPos");
    }   

    private void PetTest() {
        //Item.Add(new Item(10205, 1000));
        //Player.instance.gameData.petStorage.Add(new Pet(1,1));
        //Panel.OpenPanel("Noob");
        //Player.instance.gameData.petStorage.Add(new Pet(301,100));
    }

    public void SetPlantPanelActive(bool active) {
        plantController?.gameObject.SetActive(active);
        if (!active)
            Player.SetSceneData("seed", 0);
    }

}