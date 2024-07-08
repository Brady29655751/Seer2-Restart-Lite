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

    private void Start()
    {
        Init();
    }

    public void Init() {
        PetTest();
        LoadMap();
        LoadPlayer();
    }

    private void LoadMap() {
        playerController.SetMap(map);
        sceneController.SetMap(map);
        UIController.SetMap(map);
    }

    private void LoadPlayer() {
        playerController.SetPlayerName(Player.instance.nickname);

        Vector2 initPos = (Vector2)Player.GetSceneData("mapInitPos", map.initPoint);
        playerController.SetPlayerPosition(initPos);
        Player.RemoveSceneData("mapInitPos");
    }   

    private void PetTest() {
        //Item.Add(new Item(10205, 1000));
        //Player.instance.gameData.petStorage.Add(new Pet(90,1));
        //Panel.OpenPanel("YiTeRogue");
        //Player.instance.gameData.petStorage.Add(new Pet(301,100));
    }

}