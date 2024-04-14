using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hintbox : Panel
{
    [SerializeField] protected HintboxController hintboxController;

    public static Hintbox OpenHintbox() {
        return Hintbox.OpenHintbox<Hintbox>();
    }
    public static Hintbox OpenHintboxWithContent(string content, int fontsize) {
        var hintbox = Hintbox.OpenHintbox();
        hintbox.SetTitle("提示");
        hintbox.SetContent(content, fontsize, FontOption.Arial);
        hintbox.SetOptionNum(1);
        return hintbox;
    }

    public static T OpenHintbox<T>() where T : Hintbox {
        T panel = Panel.OpenPanel<T>();
        return panel;        
    }

    public void SetHintboxActive(bool active) {
        GameObject topLayer = (background == null) ? gameObject : background.gameObject;
        topLayer.SetActive(active);
    }

    public override void SetPanelIdentifier(string id, string param) {
        switch (id) {
            default:
                base.SetPanelIdentifier(id, param);
                break;
            case "title":
                SetTitle(param);
                break;
            case "content":
                SetContent(param, 14, FontOption.Arial);
                break;
            case "option_num":
                SetOptionNum(int.Parse(param));
                break;
        }
    }

    public void SetOptionNum(int num) {
        hintboxController.SetOptionNum(num);
    }

    public void SetSize(int x, int y) {
        hintboxController.SetSize(x, y);
    }

    public void SetTitle(string text = "提示", int fontsize = 20, FontOption font = FontOption.Zongyi) {
        hintboxController.SetTitle(text, fontsize, font);
    }

    public void SetContent(string text, int fontsize, FontOption font) {
        hintboxController.SetContent(text, fontsize, font);
    }

    public void SetOptionCallback(Action callback, bool isConfirm = true) {
        hintboxController.SetOptionCallback(callback, isConfirm);
    }

}
