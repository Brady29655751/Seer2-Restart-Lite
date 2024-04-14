using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapPetCornerController : UIModule
{
    [SerializeField] private MapPetCornerModel petCornerModel;
    [SerializeField] private MapPetCornerView petCornerView;
    
    public void SetPet(Pet pet) {
        petCornerModel.SetPet(pet);
        petCornerView.SetPet(petCornerModel.firstPet);
    }

    public void SetInfoPromptText(string content) {
        infoPrompt.SetInfoPromptWithAutoSize(content, TextAnchor.MiddleCenter);
        infoPrompt.SetPosition(new Vector2(10, -25));
    }

    public void OpenPetBagPanel() {
        PetBagPanel panel = Panel.OpenPanel<PetBagPanel>();
        panel.onCloseEvent += () => { SetPet(panel.petBag?.FirstOrDefault() ); };
    }

    public void Heal() {
        petCornerModel.Heal();
        petCornerView.OnAfterHeal();
        petCornerView.SetPet(petCornerModel.firstPet);
    }

    public void Extend() {
        petCornerView.Extend();
    }
}
