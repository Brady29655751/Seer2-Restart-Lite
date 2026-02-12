using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

public class BattleResourceView : BattleBaseView
{
    public ResourceManager RM => ResourceManager.instance;
    [SerializeField] private SpriteRenderer background;
    [SerializeField] private GameObject backgroundAnim;
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
        if (backgroundAnim != null)
        {
            Destroy(backgroundAnim);
            backgroundAnim = null;   
        }

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

        var originalPath = options.Get("background_path");
        var originalMod = options.Get("background_mod");

        var path = currentState.options.Get("background_path");
        var mod = currentState.options.Get("background_mod");

        if ((path == originalPath) && (mod == originalMod))
            return;

        options.Set("background_path", path);
        options.Set("background_mod", mod);

        if (path.TryTrimStart("Maps/", out var mapIDExpr))
        {
            var mapID = (int)Identifier.GetNumIdentifier(mapIDExpr);
            Map.GetMap(mapID, (map) => SetBackgroundAnim(map));
            return;
        }

        ResourceManager.instance.GetLocalAddressables<Sprite>(path, bool.Parse(mod), (sprite) => SetBackground(sprite));
    }

    private void SetBackgroundAnim(Map map)
    {
        var resources = map.resources;
        var prefab = resources.anim;
        GameObject mapAnim = null;

        if (prefab == null)
        {
            background.sprite = resources.bg;
            background.color = map.dream ? Color.gray : map.backgroundColor;
        } 
        else
        {
            mapAnim = Instantiate(prefab, Camera.main.transform);
            mapAnim.transform.localScale = map.anim?.animScale ?? Vector2.one;
            mapAnim.transform.localPosition = map.anim?.animPos ?? Vector2.zero;
            mapAnim.transform.SetAsFirstSibling();
            background.color = Color.clear;   
        }

        if (backgroundAnim != null)
            Destroy(backgroundAnim);  

        backgroundAnim = mapAnim;
    }
}