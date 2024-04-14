using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapPetCornerModel : Module
{
    public Player player => Player.instance;
    public Pet firstPet { get; private set; }

    public void SetPet(Pet pet) {
        firstPet = pet;
    }   

    public void Heal() {
        foreach (var p in player.petBag) {
            if (p != null) {
                p.currentStatus.hp = p.normalStatus.hp;
            }
        }
        SetPet(player.petBag.FirstOrDefault());
    }   
}
