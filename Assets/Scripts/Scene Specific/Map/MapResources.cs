using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapResources
{
    public Sprite bg;
    public Sprite pathSprite;
    public Vector2Int pathSize;
    public Texture2D pathTexture;
    public AudioClip bgm, fx;

    public MapResources(Sprite bg, Sprite pathSprite, AudioClip bgm, AudioClip fx = null) {
        this.bg = bg;
        this.pathSprite = pathSprite;
        this.pathTexture = pathSprite.texture;
        this.pathSize = new Vector2Int(pathSprite.texture.width, pathSprite.texture.height);
        this.bgm = bgm;
        this.fx = fx;
    }
}
