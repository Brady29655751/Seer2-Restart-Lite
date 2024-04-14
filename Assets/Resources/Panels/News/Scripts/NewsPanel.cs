using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class NewsPanel : Panel
{
    [SerializeField] private NewsView newsView;

    public override void Init() {
        newsView?.SetContent(GetNews());
    }

    private string GetNews() {
        var news = GameManager.versionData.releaseNote.GetDescription();
        /*
        if (Player.instance.gameDataId >= 0) {
            news += "\n【存档位置】\n" + SaveSystem.savePath;
        } 
        */  
        return news;
    }
}
