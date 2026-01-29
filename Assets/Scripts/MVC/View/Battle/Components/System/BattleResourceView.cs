using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

public class BattleResourceView : BattleBaseView
{
    public ResourceManager RM => ResourceManager.instance;
    [SerializeField] private SpriteRenderer background;
    private Dictionary<string, string> options = new Dictionary<string, string>();

    public override void Init()
    {
        LoadResources();
    }

    private void LoadResources()
    {
        LoadBackground();
        LoadDamage();
    }

    private void LoadBackground()
    {
        Sprite fightMap = (Sprite)Player.GetSceneData("fightBg");
        bool fightMapIsMod = (bool)Player.GetSceneData("fightBgIsMod");
        if (fightMap == null)
            return;

        SetBackground(fightMap);
    }

    private void SetBackground(Sprite fightMap)
    {
        background.sprite = fightMap;
        background.gameObject.transform.localScale = new Vector3(1920 / background.sprite.rect.width,
            1080 / background.sprite.rect.height, 1);
        // 修正位置
        background.gameObject.transform.position = new Vector3(-9.6f, -5.4f,
            background.transform.position.z);
        background.color = Player.instance.currentMap.fightMapColor;
    }

    private void LoadDamage()
    {
        string[] damageBgType = new string[5] { "miss", "absorb", "weak", "resist", "normal" };
        for (int i = 0; i < 5; i++)
            RM.LoadSprite("Fight/damage " + damageBgType[i]);

        for (int i = 0; i < RM.numString.Length; i++)
            RM.LoadSprite("Numbers/Damage/" + RM.numString[i].ToString());
    }

    public void SetState(BattleState lastState, BattleState currentState)
    {
        if (currentState == null)
            return;

        var path = currentState.options.Get("background_path");
        var mod = currentState.options.Get("background_mod");
        if ((path == options.Get("background_path")) && (mod == options.Get("background_mod")))
            return;

        ResourceManager.instance.GetLocalAddressables<Sprite>(path, bool.Parse(mod), (sprite) =>
        {
            options.Set("background_path", path);
            options.Set("background_mod", mod);
            SetBackground(sprite);
        });
    }
}