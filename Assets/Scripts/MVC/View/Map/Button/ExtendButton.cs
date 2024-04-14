using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExtendButton : IButton
{
    [SerializeField] private Sprite openSprite;
    [SerializeField] private Sprite closeSprite;

    public void SetMode(bool isExtendable) {
        SetSprite(isExtendable ? openSprite : closeSprite);
    }

}
