using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MiniGame;

public class MiniGameManager : Manager<MiniGameManager>
{
    protected override void Awake()
    {
        base.Awake();
        MiniGame.MiniGame.LoadMiniGame(Player.instance.currentMiniGame?.id);
    }
}
