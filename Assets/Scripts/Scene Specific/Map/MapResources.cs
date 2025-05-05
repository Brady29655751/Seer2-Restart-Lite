using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapResources
{
    public Sprite bg;
    public Sprite pathSprite;
    public Vector2Int pathSize;
    public Texture2D pathTexture;
    public AudioClip bgm, fx;
    public GameObject anim;

    public Sprite pathMaskSprite => pathSprite ?? SpriteSet.Empty;

    public MapResources(Sprite bg, Sprite pathSprite, AudioClip bgm, AudioClip fx = null, GameObject anim = null) {
        this.bg = bg;
        this.pathSprite = pathSprite;
        this.pathTexture = pathMaskSprite.texture;
        this.pathSize = new Vector2Int(pathMaskSprite.texture.width, pathMaskSprite.texture.height);
        this.bgm = bgm;
        this.fx = fx;
        this.anim = anim;
    }
}
