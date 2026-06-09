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
        var note = "<b><size=26><color=#52e5f9>本游戏完全免费，闲鱼上的卖家都是骗子，被骗请立即举报！！！</color></size></b>\n\n";
        return note + news;
    }
}
