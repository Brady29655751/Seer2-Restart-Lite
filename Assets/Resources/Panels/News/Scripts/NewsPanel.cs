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
        var note = "<b><size=20><color=#52e5f9>由于游戏可能有闪退、安卓权限、Mod操作等不可预期的情况导致存档毁损，请时常前往「设定＞其他＞导出存档」备份！特别是手机版！！！</color></size></b>\n\n";
        return note + news;
    }
}
