using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapUIController : Module
{
    [SerializeField] private MapInfoController infoController;
    [SerializeField] private MapPetCornerController petCornerController;
    [SerializeField] private MapMenuController menuController;
    [SerializeField] private MapWorldController worldController;

    public void SetMap(Map map) {
        infoController.SetMapInfoText(map.name);
        infoController.SetWeather(map.weather);
        infoController.SetDayNightSwitch(map.dayNightSwitch);
        
        petCornerController.SetPet(Player.instance.petBag.FirstOrDefault());
    }
}
