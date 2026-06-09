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
    [SerializeField] private MapAnimalController animalController;

    private void Start()
    {
        Init();
    }

    public void Init()
    {
        PetTest();
        LoadMap();
        LoadPlayer();
        SetPlantPanelActive(false);
        SetAnimalPanelActive(false);
    }

    private void LoadMap()
    {
        playerController.SetMap(map);
        sceneController.SetMap(map);
        UIController.SetMap(map);
    }

    private void LoadPlayer()
    {
        Player.instance.isShootMode = false;
        playerController.SetPlayerName(Player.instance.nickname);
        playerController.SetPlayerAchievement(Item.GetItemInfo(Player.instance.gameData.achievement)?.name ?? string.Empty);

        Vector2 initPos = (Vector2)Player.GetSceneData("mapInitPos", map.initPoint);
        playerController.SetPlayerPosition(initPos);
        Player.RemoveSceneData("mapInitPos");
    }

    private void PetTest()
    {
        //Item.Add(new Item(10205, 1000));
        //Player.instance.gameData.petStorage.Add(new Pet(1,1));
        //Panel.OpenPanel("Noob");
        //Player.instance.gameData.petStorage.Add(Pet.GetExamplePet(14761));
    }

    public void SetPlantPanelActive(bool active)
    {
        plantController?.gameObject.SetActive(active);
        if (!active)
            Player.SetSceneData("seed", 0);
    }

    public void SetAnimalPanelActive(bool active)
    {
        animalController?.gameObject.SetActive(active);
        if (!active)
            Player.SetSceneData("seed", 0);
    }

    public void RefreshPlantPanel(Item seed = null)
    {
        plantController?.Refresh();
        if (seed == null)
            return;

        plantController?.OnSelectSeed(seed);
    }

    public void RefreshAnimalPanel(Item animal = null)
    {
        animalController?.Refresh();
        if (animal == null)
            return;
        
        animalController?.OnSelectAnimal(animal);
    }
}