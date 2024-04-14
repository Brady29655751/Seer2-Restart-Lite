using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CreateRoleView : Module
{
    [SerializeField] private GameObject createRoleModule;
    [SerializeField] private Image[] genderImage = new Image[2];
    
    public void SetActive(bool active) {
        createRoleModule.SetActive(active);
    }

    public void SetGender(bool gender) {
        int chosen = gender ? 1 : 0;
        genderImage[chosen].color = Color.white;
        genderImage[1 - chosen].color = new Color(1, 1, 1, 0.5f);
    }

    public Hintbox OnSetPlayerNameEmpty() {
        Hintbox hintbox = Hintbox.OpenHintbox();
        hintbox.SetTitle("提示");
        hintbox.SetContent("昵称不能空白", 20, FontOption.Zongyi);
        return hintbox;
    }
}
